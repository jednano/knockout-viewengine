using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Jint;

namespace Knockout
{
	/// <summary>
	/// Binds a Knockout template with data.
	/// </summary>
	public class BoundView : HtmlDocument
	{
		#region Regular expressions
		/// <summary>
		/// Parses the data-bind attribute, looking for attribute/value combos.
		/// </summary>
		private static readonly Regex DataBindKeyValue = new Regex(
			@"(?<Key>[^:,\s]+):\s*(?<Value>[^,]+)", RegexOptions.Compiled);

		/// <summary>
		/// Identifies action attributes (i.e. attributes who's values are to perform an action
		/// on the client). These attributes can be safely skipped for serving static,
		/// data-bound content.
		/// </summary>
		private static readonly Regex SkipRule = new Regex(
			"^(click|submit|valueUpdate)$", RegexOptions.Compiled);

		private static readonly Regex TraverseRule = new Regex(
			@",?\s*(?<Key>foreach|simpleGrid):\s*(?<Value>.+)(?!,s*[a-z]+:)",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// Any number of tabs and spaces at the end of a string.
		/// </summary>
		private static readonly Regex TabsAndSpaces = new Regex(
			@"\s*$", RegexOptions.Compiled);

		/// <summary>
		/// The entire string is composed of tabs, spaces and/or newlines.
		/// </summary>
		private static readonly Regex TabsSpacesNewLines = new Regex(
			@"^\s+$", RegexOptions.Compiled);

		/// <summary>
		/// Matches the <!-- ko --> start marker, defining a virtual element.
		/// </summary>
		private static readonly Regex VirtualElementBegin = new Regex(
			@"<!--\s*ko\s*(?<Rules>.*[^\s])\s*-->", RegexOptions.Compiled);

		/// <summary>
		/// Matches the <!-- /ko --> end marker, defining the end of a virtual element.
		/// </summary>
		private static readonly Regex VirtualElementEnd = new Regex(
			@"<!--\s*/ko\s*-->", RegexOptions.Compiled);

		/// <summary>
		/// Matches any word leading up to and including "$root."
		/// </summary>
		private static readonly Regex RootPat = new Regex(@"[^\s]*\$root\.",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// Matches "$parent." and "$parents[n]."
		/// </summary>
		private static readonly Regex ParentPat = new Regex(@"(\$parent(?:s\[(?<Index>\d+)\])?\.)+",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// Matches digits immediately following "$parents"
		/// </summary>
		private static readonly Regex ParentsPat = new Regex(@"(?<=\$parents)(\d+)", RegexOptions.Compiled);

		/// <summary>
		/// Matches reserved Javascript keywords.
		/// </summary>
		private static readonly Regex ReservedKeywords = new Regex(
			@"^(break|case|catch|continue|const|default|delete|do|else|export|false|finally|for|function|if|import|in|instanceOf|ko|label|let|new|return|switch|this|throw|try|true|typeof|var|void|while|with|yield)$",
			RegexOptions.Compiled);

		/// <summary>
		/// Matches keywords, vars or function names in code.
		/// </summary>
		private static readonly Regex Word = new Regex(
			@"(.)?\b(?<Word>(?<c1>[a-z_])([a-z_]*))\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private static readonly Regex DotOrDollar = new Regex(@"[\.\$]", RegexOptions.Compiled);

		/// <summary>
		/// The rule by which bindings can safely be split apart.
		/// </summary>
		private static readonly Regex SplitBinding = new Regex(
			@"\s*,\s*(?=[a-z]+:)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		#endregion

		private readonly Stack<object> _data;
		private readonly Dictionary<string, string> _styles;


		/// <summary>
		/// Binds a Knockout template with data.
		/// </summary>
		/// <param name="view">The Knockout template html.</param>
		/// <param name="data">The data to be bound to the template.</param>
		public BoundView(string view, object data = null)
		{
			// Before parsing the HTML into an HtmlDocument, replace all HTML Knockout
			// comments with <ko> tags instead, making HTML much easier to process
			// rules with looping and such.
			view = VirtualElementBegin.Replace(view, m =>
				string.Format(@"<ko data-bind=""{0}"">", m.Groups["Rules"].Value));
			view = VirtualElementEnd.Replace(view, "</ko>");

			LoadHtml(view);
			if (data == null) return;
			_data = new Stack<object>();
			_styles = new Dictionary<string, string>();
			TraverseBindData(DocumentNode, data);
		}

		private static string GenericToString(object value)
		{
			return value is IEnumerable<string>
				? string.Join(",", ((IEnumerable<string>)value).ToArray())
				: value.ToString();
		}

		private static void SetSelectedValue(HtmlNode node, object value)
		{
			foreach (var option in from option in node.Descendants("option")
								   where option.InnerText == (string)value
								   select option)
				option.Attributes.Add("selected", "selected");
		}

		private object ProcessValue(string val)
		{
			var engine = new JintEngine();
			engine.SetParameter("$root", _data.Last());
			engine.SetParameter("$data", _data.Peek());
			engine.SetParameter("ko", Ko.Instance);
			if (_data.Count > 1)
				engine.SetParameter("$parent", _data.ElementAt(1));

			// TODO: Support jQuery and Knockout libraries.
			//engine.Run(Properties.Resources.jquery);
			//engine.Run(Properties.Resources.knockout);

			// Simplify $root vars by ignoring $parent(s) leading up to it.
			val = RootPat.Replace(val, m => "$root.");

			#region Handle $parent chains and $parents[i] pseudovariables.
			// Simplify $parent(s) chains into a single $parents[generations] variable.
			val = ParentPat.Replace(val, m => string.Format("$parents{0}.",
			                                                    m.Groups["Index"].Captures.Cast<Capture>()
			                                                    	.Sum(c => int.Parse(c.Value) - 1) +
			                                                    m.Groups[1].Captures.Count));
			// Assign the appropriate data value from the stack.
			foreach (var m in from Match m in ParentsPat.Matches(val) select m.Groups[1].Value)
				engine.SetParameter("$parents" + m, _data.ElementAt(_data.Count - int.Parse(m)));
			#endregion

			// 1. Find non-js keyword members and prefix them with "$data."
			// 2. Capitalize the first letter after a dot or space to conform to C# naming conventions.
			val = Word.Replace(val, m => (ReservedKeywords.IsMatch(m.Groups["Word"].Value) || m.Groups[1].Value == "$")
			                             	? m.Value
			                             	: string.Concat(m.Groups[1].Value,
			                             	                DotOrDollar.IsMatch(m.Groups[1].Value) ? string.Empty : "$data.",
			                             	                m.Groups["c1"].Value.ToUpper(),
			                             	                m.Groups[2].Value));
			return engine.Run(val);
		}

		/// <summary>
		/// LINQ to XML alternative to XPath ".//*[@data-bind]" expression, which should
		/// yield better performance. Also selects virtual elements (i.e. <!-- ko -->).
		/// </summary>
		public IEnumerable<HtmlNode> SelectDataBindElements(HtmlNode node)
		{
			return from n in node.Descendants()
			       where n.Attributes["data-bind"] != null
			       select n;
		}

		/// <summary>
		/// Simple, non-recursive data binding.
		/// </summary>
		/// <returns>The data-bound parentNode.</returns>
		private HtmlNode SimpleBindData(HtmlNode parentNode)
		{
			foreach (var node in SelectDataBindElements(parentNode))
			{
				var bindings = new Dictionary<string, object>();
				var dataBind = node.Attributes["data-bind"];

				foreach (var binding in from b in SplitBinding.Split(dataBind.Value)
										select b.TrimEnd())
				{
					var m = DataBindKeyValue.Match(binding);
					if (!m.Success)
						throw new FormatException(string.Format(
							@"Binding ""{0}"" does not meet the Knockout specification.", binding));

					var key = m.Groups["Key"].Value;
					var val = m.Groups["Value"].Value;

					// If an action attribute is identified or the value is not a simple property name
					// then we will assume it is intended to be interpreted by Knockout.js on the client.
					if (SkipRule.IsMatch(key))
					{
						bindings.Add(key, val);
						continue;
					}

					try
					{
						var value = ProcessValue(val);

						// Don't process empty or null values and don't leave them in the bindings either.
						if (value is string && string.IsNullOrEmpty((string)value))
							continue;

						#region Handle Knockout built-in bindings.
						switch (key)
						{
							#region Controlling text and appearance
							case "visible":
								if (!(bool)value) _styles.Add("display", "none");
								break;

							case "text":
								goto case "html";

							case "html":
								node.InnerHtml = GenericToString(value);
								break;

							//case "css":
							//	break;

							//case "style":
							//	break;

							//case "attr":
							//	break;
							#endregion

							#region Control flow
							//case "if":
							//	break;

							//case "ifnot":
							//	break;

							//case "with":
							//	break;
							#endregion

							#region Working with form fields
							case "enable":
								if (!(value is bool ? (bool)value
									: Convert.ToBoolean(value))) goto case "_disabled";
								break;

							case "disable":
								if (value is bool ? (bool)value
									: Convert.ToBoolean(value)) goto case "_disabled";
								break;

							case "_disabled":
								node.Attributes.Add("disabled", "disabled");
								break;

							case "value":
								var name = node.Name.ToLower();
								if (name == "textarea") goto case "text";
								if (name == "select") SetSelectedValue(node, value);
								goto default;

							case "checked":
								if (value is string)
								{
									if (node.Attributes["value"].Value == value.ToString())
										goto case "_checked";
									break;
								}
								if (value is bool ? (bool)value : Convert.ToBoolean(value))
									goto case "_checked";
								break;

							case "_checked":
								node.Attributes.Add("checked", "checked");
								break;

							case "options":
								var options = new List<HtmlNode>();
								foreach (var text in (IEnumerable<string>)value)
								{
									var option = CreateElement("option");
									option.InnerHtml = text;
									options.Add(option);
								}
								ReplaceAllChildren(node, options);
								break;

							case "selectedOptions":
								foreach (var v in ((IEnumerable<string>)value))
									SetSelectedValue(node, v);
								break;
							#endregion

							#region Rendering templates
							//case "template":
							//	break;
							#endregion

							default:
								node.Attributes.Add(key, GenericToString(value));
								break;
						}
						#endregion

					}
					catch (JintException)
					{
						bindings.Add(key, val);
					}
				}

				// Any bindings left over that we skipped or haven't processed?
				if (bindings.Any())
					dataBind.Value = string.Join(", ", bindings.Select(
						b => string.Join(": ", b.Key, b.Value)).ToList());
				else
					dataBind.Remove();

				// Join all styles into a single string.
				if (!_styles.Any()) continue;
				node.Attributes.Add("style", string.Join("; ", _styles.Select(
					s => string.Join(": ", s.Key, s.Value)).ToList()));
				_styles.Clear();

				SimplifyKoTag(node);
			}
			
			return parentNode;
		}

		/// <summary>
		/// Complex, recursive data binding.
		/// </summary>
		/// <returns>The data-bound parentNode.</returns>
		public HtmlNode TraverseBindData(HtmlNode parentNode, object data)
		{
			_data.Push(data);
			while (true)
			{
				// At first glance, this might seem like a strange way of going about traversing
				// a node hierarchy; however, the challenge here was to data-bind the deepest
				// "foreach:" nodes before the shallow ones. Only after all "foreach:" nodes are
				// handled is it safe to break out of this while loop and simple-bind the root node.
				var node = (from n in SelectDataBindElements(parentNode)
				            where (TraverseRule.IsMatch(n.Attributes["data-bind"].Value))
				            select n).FirstOrDefault();
				if (node == null) break;

				var dataBind = node.Attributes["data-bind"];
				var m = TraverseRule.Match(dataBind.Value);

				var val = m.Groups["Value"].Value;
				object value;
				try
				{
					value = ProcessValue(val);
				}
				catch (JintException)
				{
					break;
				}

				// Being careful to remove only the "[traverseRule]:" binding rules and leave any others
				// untouched for the possibility of simple binding, which will take place at the end.
				dataBind.Value = TraverseRule.Replace(dataBind.Value, string.Empty);
				if (dataBind.Value == string.Empty) dataBind.Remove();

				switch (m.Groups["Key"].Value)
				{
					case "simpleGrid":
						node.InnerHtml = new BoundView(
								Properties.Resources.SimpleGridView, value)
								.DocumentNode.InnerHtml;
						break;

					default:
						// Clone the template and bind data for every iteration.
						var template = node.Clone();
						node.RemoveAllChildren();

						// Clean template of any leading/trailing tabs, spaces and newlines.
						if (template.FirstChild.NodeType == HtmlNodeType.Text
							&& TabsSpacesNewLines.IsMatch(template.FirstChild.InnerText))
							template.FirstChild.Remove();
						if (template.LastChild.NodeType == HtmlNodeType.Text
							&& TabsSpacesNewLines.IsMatch(template.LastChild.InnerText))
							template.LastChild.Remove();

						// Replace all children with newly-bound children.
						var childNodes = (from childData in ((IEnumerable<object>)value)
										  let tClone = template.Clone()
										  select TraverseBindData(tClone, childData))
							.SelectMany(boundChild => boundChild.ChildNodes).ToList();
						ReplaceAllChildren(node, childNodes);
						break;
				}

				SimplifyKoTag(node);
			}
			
			// At this point, all the "foreach:" rules below the parentNode will have been
			// processed and removed from the data-bind attributes. The only binding left
			// to be done is simple, non-recursive binding.
			var result = SimpleBindData(parentNode);
			_data.Pop();
			return result;
		}

		/// <summary>
		/// Converts ko tag into a ko comment (virtual element) if bindings still exist; otherwise,
		/// removes the tag entirely, but preserves the child nodes in the process.
		/// </summary>
		/// <param name="node">The ko element.</param>
		private static void SimplifyKoTag(HtmlNode node)
		{
			if (node.Name != "ko") return;
			var parentNode = node.ParentNode;
			if (node.HasAttributes)
				parentNode.InsertBefore(HtmlNode.CreateNode(
					string.Format("<!-- ko {0} -->", node.Attributes["data-bind"].Value)), node);
			foreach (var cn in node.ChildNodes)
				parentNode.InsertBefore(cn, node);
			if (node.HasAttributes)
				parentNode.InsertBefore(HtmlNode.CreateNode("<!-- /ko -->"), node);
			node.Remove();
		}

		/// <summary>
		/// Replaces all child nodes with the supplied nodes and indents them +1 tab.
		/// </summary>
		private void ReplaceAllChildren(HtmlNode parent, IEnumerable<HtmlNode> nodes)
		{
			// Preserve the indentation of the parent.
			var parentScope = CreateTextNode();
			var prev = parent.PreviousSibling;
			if (prev == null && parent.ParentNode != null)
				prev = parent.ParentNode.PreviousSibling;
			if (prev != null && prev.NodeType == HtmlNodeType.Text)
			{
				var m = TabsAndSpaces.Match(prev.InnerText);
				if (m.Success)
					parentScope.InnerHtml += m.Value;
			}

			// Add one tab of indentation for the children.
			var childScope = CreateTextNode(parentScope.InnerText + "\t");

			// Replace all children with supplied nodes (indented).
			parent.RemoveAllChildren();
			foreach (var option in nodes)
			{
				parent.AppendChild(childScope);
				parent.AppendChild(option);
			}

			// Put the closing tag at the same indentation as the opening tag.
			parent.AppendChild(parentScope);
		}
	}
}
