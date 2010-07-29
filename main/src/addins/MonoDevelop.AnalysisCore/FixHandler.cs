// 
// FixHandler.cs
//  
// Author:
//       Michael Hutchinson <mhutchinson@novell.com>
// 
// Copyright (c) 2010 Novell, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using MonoDevelop.Refactoring.Rename;
using MonoDevelop.Refactoring;
using MonoDevelop.Core;
using MonoDevelop.Ide;

namespace MonoDevelop.AnalysisCore
{
	public interface IFixHandler
	{
		IEnumerable<IAnalysisFixAction> GetFixes (MonoDevelop.Ide.Gui.Document doc, object fix);
	}
	
	public interface IAnalysisFixAction
	{
		string Label { get; }
		void Fix ();
	}
	
	public class RenameMemberHandler : IFixHandler
	{
		//FIXME: why is this invalid on the parseddocuments loaded when the doc is first loaded?
		//maybe the item's type's SourceProject is null?
		public IEnumerable<IAnalysisFixAction> GetFixes (MonoDevelop.Ide.Gui.Document doc, object fix)
		{
			var renameFix = (RenameMemberFix) fix;
			var refactoring = new RenameRefactoring ();
			var options = new RefactoringOptions () {
				Document = doc,
				Dom = doc.Dom,
				SelectedItem = renameFix.Item,
			};
			if (!refactoring.IsValid (options))
				yield break;
			
			var prop = new RenameRefactoring.RenameProperties () {
				NewName = renameFix.NewName,
			};
			
			yield return new RenameFixAction () {
				Label = GettextCatalog.GetString ("Rename '{0}' to '{1}'", renameFix.Item.Name, renameFix.NewName),
				Refactoring = refactoring,
				Options = options,
				Properties = prop,
				Preview = false,
			};
				
			yield return new RenameFixAction () {
				Label = GettextCatalog.GetString ("Rename '{0}' to '{1}' with preview",
					renameFix.Item.Name, renameFix.NewName),
				Refactoring = refactoring,
				Options = options,
				Properties = prop,
				Preview = true,
			};
		}
		
		class RenameFixAction : IAnalysisFixAction
		{
			public RenameRefactoring Refactoring;
			public RefactoringOptions Options;
			public RenameRefactoring.RenameProperties Properties;
			public bool Preview;
			public string Label { get; set; }
			
			public void Fix ()
			{
				//FIXME: performchanges should probably use a monitor too, as it can be slow
				var changes = Refactoring.PerformChanges (Options, Properties);
				if (Preview) {
					MessageService.ShowCustomDialog (new RefactoringPreviewDialog (Options.Dom, changes));
				} else {
					var monitor = IdeApp.Workbench.ProgressMonitors.GetBackgroundProgressMonitor ("Rename", null);
					RefactoringService.AcceptChanges (monitor, Options.Dom, changes);
				}
			}
		}
	}
}

