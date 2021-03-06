// NewFileDialog.cs
//
// Author:
//   Todd Berman  <tberman@off.net>
//   Viktoria Dudka  <viktoriad@remobjects.com>
//
// Copyright (c) 2004 Todd Berman
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
//
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

using MonoDevelop.Ide.Gui.Dialogs;
using MonoDevelop.Core;
using Mono.Addins;
using MonoDevelop.Ide.Templates;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;

using Gtk;
using MonoDevelop.Components;
using IconView = MonoDevelop.Components.IconView;
using Gdk;
using MonoDevelop.Ide.Gui.Components;

namespace MonoDevelop.Ide.Projects
{
	/// <summary>
	///  This class is for creating a new "empty" file
	/// </summary>
	internal partial class NewFileDialog : Dialog
	{
		List<TemplateItem> alltemplates = new List<TemplateItem> ();
		List<Category> categories = new List<Category> ();
		Dictionary<string, bool> activeLangs = new Dictionary<string, bool> ();

		TreeStore catStore;
		TemplateView iconView;

		// Add To Project widgets
		string[] projectNames;
		Project[] projectRefs;

		Project parentProject;
		string basePath;

		string userEditedEntryText = null;
		string previousDefaultEntryText = null;

		public NewFileDialog (Project parentProject, string basePath)
		{
			Build ();
			this.parentProject = parentProject;
			this.basePath = basePath;

			BorderWidth = 6;
			TransientFor = IdeApp.Workbench.RootWindow;
			HasSeparator = false;

			InitializeComponents ();

			nameEntry.GrabFocus ();

		}


		void InitializeView ()
		{
			InsertCategories (TreeIter.Zero, categories);

			TreeIter treeIter;
			if (!FindCatIter (PropertyService.Get (GetCategoryPropertyKey (parentProject), "General"), out treeIter)) {
				if (!FindCatIter ("Misc", out treeIter))
					if (!catStore.GetIterFirst (out treeIter))
						return;
			}
			catView.Selection.SelectIter (treeIter);
		}

		void InsertCategories (TreeIter node, List<Category> catarray)
		{
			foreach (Category category in catarray) {
				if (TreeIter.Zero.Equals (node))
					InsertCategories (catStore.AppendValues (category.Name, category.Categories, category.Templates), category.Categories);
				else
					InsertCategories (catStore.AppendValues (node, category.Name, category.Categories, category.Templates), category.Categories);
			}
		}

		Category GetCategory (string categoryname)
		{
			return GetCategory (categories, categoryname);
		}

		Category GetCategory (List<Category> catList, string categoryname)
		{
			foreach (Category category in catList) {
				if (category.Name == categoryname)
					return category;
			}

			Category cat = new Category (categoryname);
			catList.Add (cat);
			return cat;

		}

		void CategoryChange (object sender, EventArgs e)
		{
			TreeModel treeModel;
			TreeIter treeIter;

			if (catView.Selection.GetSelected (out treeModel, out treeIter)) {
				okButton.Sensitive = false;
				FillCategoryTemplates (treeIter);
				catView.ExpandRow (treeModel.GetPath (treeIter), false);
			}
		}



		void InitializeDialog (bool update)
		{
			if (update) {
				alltemplates.Clear ();
				categories.Clear ();
				catStore.Clear ();
				activeLangs.Clear ();
			}

			InitializeTemplates ();

			if (update) {
				iconView.Clear ();
				InitializeView ();
			}
		}


		protected override void OnDestroyed ()
		{
			if (catStore != null) {
				catStore.Dispose ();
				catStore = null;
			}
			base.OnDestroyed ();
		}

		static string GetCategoryPropertyKey (Project proj)
		{
			string key = "Dialogs.NewFileDialog.LastSelectedCategory";
			if (proj != null) {
				key += "." + proj.ProjectType;
				if (proj is DotNetProject)
					key += "." + ((DotNetProject)proj).LanguageName;
			}
			return key;
		}

		bool FindCatIter (string catPath, out TreeIter iter)
		{
			string[] cats = catPath.Split ('/');
			iter = TreeIter.Zero;

			TreeIter nextIter;
			if (!catStore.GetIterFirst (out nextIter))
				return false;

			for (int i = 0; i < cats.Length; i++) {
				if (FindCategoryAtCurrentLevel (cats[i], ref nextIter)) {
					iter = nextIter;
					if (i >= cats.Length - 1 || !catStore.IterChildren (out nextIter, nextIter))
						return true;
				}
			}
			return false;
		}

		bool FindCategoryAtCurrentLevel (string category, ref TreeIter iter)
		{
			TreeIter trial = iter;
			do {
				string val = (string)catStore.GetValue (trial, 0);
				if (val == category) {
					iter = trial;
					return true;
				}
			} while (catStore.IterNext (ref trial));
			return false;
		}

		string GetCatPath (TreeIter iter)
		{
			TreeIter currentIter = iter;
			string path = (string)catStore.GetValue (currentIter, 0);
			while (catStore.IterParent (out currentIter, currentIter)) {
				path = ((string)catStore.GetValue (currentIter, 0)) + "/" + path;
			}
			return path;
		}



		public void SelectTemplate (string id)
		{
			TreeIter iter;
			catStore.GetIterFirst (out iter);
			SelectTemplate (iter, id);
		}

		public bool SelectTemplate (TreeIter iter, string id)
		{
			do {
				foreach (TemplateItem item in (List<TemplateItem>)(catStore.GetValue (iter, 2))) {
					if (item.Template.Id == id) {
						catView.ExpandToPath (catStore.GetPath (iter));
						catView.Selection.SelectIter (iter);
						CategoryChange (null, null);
						iconView.CurrentlySelected = item;
						return true;
					}
				}

				TreeIter citer;
				if (catStore.IterChildren (out citer, iter)) {
					do {
						if (SelectTemplate (citer, id))
							return true;
					} while (catStore.IterNext (ref citer));
				}

			} while (catStore.IterNext (ref iter));
			return false;
		}



		void InitializeTemplates ()
		{
			Project project = null;

			if (!boxProject.Visible || projectAddCheckbox.Active)
				project = parentProject;
			
			var templates = FileTemplate.GetFileTemplates (project, basePath);
			templates.Sort ((FileTemplate t, FileTemplate u) => string.Compare (t.Name, u.Name));
			
			foreach (var template in templates) {
				List<string> langs = template.GetCompatibleLanguages (project, basePath);
				if (langs != null) {
					foreach (string language in langs) {
						AddTemplate (new TemplateItem (template, language), language);
						//count the number of active languages
						activeLangs[language] = true;
					}
				}

			}
		}

		void AddTemplate (TemplateItem titem, string templateLanguage)
		{
			Project project = null;
			Category cat = null;

			if (!boxProject.Visible || projectAddCheckbox.Active)
				project = parentProject;

			if (project != null) {
				if ((templateLanguage != "") && (activeLangs.Count > 2)) {
					// The template requires a language, but the project does not have a single fixed
					// language type (plus empty match), so create a language category
					cat = GetCategory (templateLanguage);
					cat = GetCategory (cat.Categories, titem.Template.Category);
				} else {
					cat = GetCategory (titem.Template.Category);
				}
			} else {
				if (templateLanguage != "") {
					// The template requires a language, but there is no current language set, so
					// create a category for it
					cat = GetCategory (templateLanguage);
					cat = GetCategory (cat.Categories, titem.Template.Category);
				} else {
					cat = GetCategory (titem.Template.Category);
				}
			}

			cat.Templates.Add (titem);

			if (cat.Selected == false && titem.Template.WizardPath == null) {
				cat.Selected = true;
			}

			if (!cat.HasSelectedTemplate && titem.Template.Files.Count == 1) {
				if (((FileDescriptionTemplate)titem.Template.Files[0]).Name.StartsWith ("Empty")) {
					//titem.Selected = true;
					cat.HasSelectedTemplate = true;
				}
			}

			alltemplates.Add (titem);
		}

		//tree view event handler for double-click
		//toggle the expand collapse methods.
		void CategoryActivated (object sender, RowActivatedArgs args)
		{
			if (!catView.GetRowExpanded (args.Path)) {
				catView.ExpandRow (args.Path, false);
			} else {
				catView.CollapseRow (args.Path);
			}
		}

		void FillCategoryTemplates (TreeIter iter)
		{
			iconView.Clear ();
			foreach (TemplateItem item in (List<TemplateItem>)(catStore.GetValue (iter, 2))) {
				iconView.Add (item);
			}
		}

		// list view event handlers
		void SelectedIndexChange (object sender, EventArgs e)
		{
			UpdateOkStatus ();
		}

		void NameChanged (object sender, EventArgs e)
		{
			UpdateOkStatus ();
		}

		string GetFileNameFromEntry ()
		{
			return nameEntry.Text.Trim ();
		}

		void UpdateOkStatus ()
		{
			try {
				TemplateItem sel = (TemplateItem)iconView.CurrentlySelected;
				if (sel == null) {
					okButton.Sensitive = false;
					infoLabel.Text = string.Empty;
					labelTemplateTitle.Text = string.Empty;
					return;
				}

				FileTemplate item = sel.Template;

				if (item != null) {
					labelTemplateTitle.Markup = "<b>" + GettextCatalog.GetString (item.Name) + "</b>";
					infoLabel.Text = GettextCatalog.GetString (item.Description);

					string filename = GetFileNameFromEntry ();

					//desensitise the text entry if the name is fixed
					//careful to store user-entered text so we can replace it if they change their selection
					if (item.IsFixedFilename) {
						if (userEditedEntryText == null)
							userEditedEntryText = filename;
						nameEntry.Text = item.DefaultFilename;
						nameEntry.Sensitive = false;
					} else {
						if (userEditedEntryText != null) {
							nameEntry.Text = userEditedEntryText;
							userEditedEntryText = null;
						}
						nameEntry.Sensitive = true;
					}

					//fill in a default name if text entry is empty or contains a default name
					if ((string.IsNullOrEmpty (filename) || (previousDefaultEntryText == filename)) && !string.IsNullOrEmpty (item.DefaultFilename)) {
						nameEntry.Text = item.DefaultFilename;
						previousDefaultEntryText = item.DefaultFilename;
					}
					
					Project project = null;
					string path = null;
					if (!boxProject.Visible || projectAddCheckbox.Active) {
						project = parentProject;
						path = basePath;
					}
					
					if (projectAddCheckbox.Active) {
						okButton.Sensitive = item.IsValidName (filename, sel.Language);
					} else {
						if (!item.IsValidName (filename, sel.Language)) {
							okButton.Sensitive = false;
						} else {
							bool sensitive = true;
							foreach (var file in item.Files) {
								if (!item.CanCreateUnsavedFiles (file, project, project, path, sel.Language, filename)) {
									sensitive = false;
									break;
								}
							}
							okButton.Sensitive = sensitive;
						}
					}
				} else {
					nameEntry.Sensitive = true;
					okButton.Sensitive = false;
					infoLabel.Text = string.Empty;
					labelTemplateTitle.Text = string.Empty;
				}
			} catch (Exception ex) {
				LoggingService.LogError (ex.ToString ());
			}
		}

		// button events

		protected void CheckedChange (object sender, EventArgs e)
		{
			//((ListView)ControlDictionary["templateListView"]).View = ((RadioButton)ControlDictionary["smallIconsRadioButton"]).Checked ? View.List : View.LargeIcon;
		}

		public event EventHandler OnOked;

		void OpenEvent (object sender, EventArgs e)
		{
			if (!okButton.Sensitive)
				return;

			//FIXME: we need to set this up
			//PropertyService.Set("Dialogs.NewProjectDialog.LargeImages", ((RadioButton)ControlDictionary["largeIconsRadioButton"]).Checked);
			TreeIter selectedIter;
			if (catView.Selection.GetSelected (out selectedIter))
				PropertyService.Set (GetCategoryPropertyKey (parentProject), GetCatPath (selectedIter));

			string filename = GetFileNameFromEntry ();
			if (iconView.CurrentlySelected != null && filename.Length > 0) {
				TemplateItem titem = (TemplateItem)iconView.CurrentlySelected;
				FileTemplate item = titem.Template;
				Project project = null;
				string path = null;

				if (!boxProject.Visible || projectAddCheckbox.Active) {
					project = parentProject;
					path = basePath;
				}

				try {
					if (!item.Create (project, project, path, titem.Language, filename))
						return;
				} catch (Exception ex) {
					MessageService.ShowException (ex);
					return;
				}

				if (project != null)
					IdeApp.ProjectOperations.Save (project);

				if (OnOked != null)
					OnOked (null, null);
				Respond (Gtk.ResponseType.Ok);
				Destroy ();
			}
		}


		/// <summary>
		///  Represents a new file template
		/// </summary>
		private class TemplateItem
		{
			public TemplateItem (FileTemplate template, string language)
			{
				this.template = template;
				this.language = language;
			}

			private string language;
			public string Language {
				get { return language; }
			}

			public string Name {
				get { return template.Name; }
			}

			private FileTemplate template = null;
			public FileTemplate Template {
				get { return template; }
			}

		}

		void cancelClicked (object o, EventArgs e)
		{
			Destroy ();
		}

		void AddToProjectToggled (object o, EventArgs e)
		{
			projectAddCombo.Sensitive = projectAddCheckbox.Active;
			projectPathLabel.Sensitive = projectAddCheckbox.Active;
			projectFolderEntry.Sensitive = projectAddCheckbox.Active;

			TemplateItem titem = (TemplateItem)iconView.CurrentlySelected;
			
			if (projectAddCheckbox.Active) {
				AddToProjectComboChanged (null, null);
			} else {
				parentProject = null;
				InitializeDialog (true);
			}

			if (titem != null)
				SelectTemplate (titem.Template.Id);
		}

		void AddToProjectComboChanged (object o, EventArgs e)
		{
			int which = projectAddCombo.Active;
			Project project = null;
			
			try {
				project = projectRefs[which];
			} catch (IndexOutOfRangeException) { }

			if (project != null) {
				if (basePath == null || basePath == String.Empty || (parentProject != null && basePath == parentProject.BaseDirectory)) {
					basePath = project.BaseDirectory;
					projectFolderEntry.Path = basePath;
				}

				parentProject = project;

				InitializeDialog (true);
			}
		}

		void AddToProjectPathChanged (object o, EventArgs e)
		{
			basePath = projectFolderEntry.Path;
		}

		void InitializeComponents ()
		{
			iconView = new TemplateView ();
			iconView.ShowAll ();
			boxTemplates.PackStart (iconView, true, true, 0);
			
			catStore = new TreeStore (typeof(string), typeof(List<Category>), typeof(List<TemplateItem>));

			TreeViewColumn treeViewColumn = new TreeViewColumn ();
			treeViewColumn.Title = "categories";
			CellRenderer cellRenderer = new CellRendererText ();
			treeViewColumn.PackStart (cellRenderer, true);
			treeViewColumn.AddAttribute (cellRenderer, "text", 0);
			catView.AppendColumn (treeViewColumn);

			catStore.SetSortColumnId (0, SortType.Ascending);
			catView.Model = catStore;

			okButton.Clicked += new EventHandler (OpenEvent);
			cancelButton.Clicked += new EventHandler (cancelClicked);

			nameEntry.Changed += new EventHandler (NameChanged);
			nameEntry.Activated += new EventHandler (OpenEvent);

			infoLabel.Text = string.Empty;
			labelTemplateTitle.Text = string.Empty;
			
			ReadOnlyCollection<Project> projects = null;
			if (parentProject == null)
				projects = IdeApp.Workspace.GetAllProjects ();

			if (projects != null && projects.Count > 0) {
				Project curProject = IdeApp.ProjectOperations.CurrentSelectedProject;

				boxProject.Visible = true;
				projectAddCheckbox.Active = curProject != null;
				projectAddCheckbox.Toggled += new EventHandler (AddToProjectToggled);

				projectNames = new string[projects.Count];
				projectRefs = new Project[projects.Count];
				int i = 0;

				bool singleSolution = IdeApp.Workspace.Items.Count == 1 && IdeApp.Workspace.Items[0] is Solution;

				foreach (Project project in projects) {
					projectRefs[i] = project;
					if (singleSolution)
						projectNames[i++] = project.Name; else
						projectNames[i++] = project.ParentSolution.Name + "/" + project.Name;
				}

				Array.Sort (projectNames, projectRefs);
				i = Array.IndexOf (projectRefs, curProject);

				foreach (string pn in projectNames)
					projectAddCombo.AppendText (pn);

				projectAddCombo.Active = i != -1 ? i : 0;
				projectAddCombo.Sensitive = projectAddCheckbox.Active;
				projectAddCombo.Changed += new EventHandler (AddToProjectComboChanged);

				projectPathLabel.Sensitive = projectAddCheckbox.Active;
				projectFolderEntry.Sensitive = projectAddCheckbox.Active;
				if (curProject != null)
					projectFolderEntry.Path = curProject.BaseDirectory;
				projectFolderEntry.PathChanged += new EventHandler (AddToProjectPathChanged);

				if (curProject != null) {
					basePath = curProject.BaseDirectory;
					parentProject = curProject;
				}
			} else {
				boxProject.Visible = false;
			}

			catView.Selection.Changed += new EventHandler (CategoryChange);
			catView.RowActivated += new RowActivatedHandler (CategoryActivated);
			iconView.SelectionChanged += new EventHandler (SelectedIndexChange);
			iconView.DoubleClicked += new EventHandler (OpenEvent);
			InitializeDialog (false);
			InitializeView ();
			UpdateOkStatus ();
		}

		protected virtual void OnScrolledInfoSizeAllocated (object o, Gtk.SizeAllocatedArgs args)
		{
			if (infoLabel.WidthRequest != scrolledInfo.Allocation.Width) {
				infoLabel.WidthRequest = scrolledInfo.Allocation.Width;
				labelTemplateTitle.WidthRequest = scrolledInfo.Allocation.Width;
			}
		}
		
		
		class Category
		{
			List<Category> categories = new List<Category> ();
			List<TemplateItem> templates = new List<TemplateItem> ();
			string name;

			public Category (string name)
			{
				this.name = name;
			}

			public string Name {
				get { return name; }
			}
			public List<Category> Categories {
				get { return categories; }
			}
			public List<TemplateItem> Templates {
				get { return templates; }
			}

			private bool selected;
			public bool Selected {
				get { return selected; }
				set { selected = value; }
			}

			private bool hasSelectedTemplate;
			public bool HasSelectedTemplate {
				get { return hasSelectedTemplate; }
				set { hasSelectedTemplate = value; }
			}
		}
		
		class TemplateView: ScrolledWindow
		{
			TemplateTreeView tree;
			
			public TemplateView ()
			{
				tree = new TemplateTreeView ();
				tree.Selection.Changed += delegate {
					if (SelectionChanged != null)
						SelectionChanged (this, EventArgs.Empty);
				};
				tree.RowActivated += delegate {
					if (DoubleClicked != null)
						DoubleClicked (this, EventArgs.Empty);
				};
				Add (tree);
				HscrollbarPolicy = PolicyType.Automatic;
				VscrollbarPolicy = PolicyType.Automatic;
				ShadowType = ShadowType.In;
				ShowAll ();
			}
			
			public TemplateItem CurrentlySelected {
				get { return tree.CurrentlySelected; }
				set { tree.CurrentlySelected = value; }
			}
			
			public void Add (TemplateItem templateItem)
			{
				tree.Add (templateItem);
			}
			
			public void Clear ()
			{
				tree.Clear ();
			}
			
			public event EventHandler SelectionChanged;
			public event EventHandler DoubleClicked;
		}
			
		class TemplateTreeView: TreeView
		{
			Gtk.ListStore templateStore;
			
			public TemplateTreeView ()
			{
				HeadersVisible = false;
				templateStore = new ListStore (typeof(string), typeof(string), typeof(TemplateItem));
				Model = templateStore;
				
				TreeViewColumn col = new TreeViewColumn ();
				CellRendererIcon crp = new CellRendererIcon ();
				crp.StockSize = (uint) Gtk.IconSize.Dnd;
				crp.Ypad = 2;
				col.PackStart (crp, false);
				col.AddAttribute (crp, "stock-id", 0);
				
				CellRendererText crt = new CellRendererText ();
				col.PackStart (crt, false);
				col.AddAttribute (crt, "markup", 1);
				
				AppendColumn (col);
				ShowAll ();
			}
			
			public TemplateItem CurrentlySelected {
				get {
					Gtk.TreeIter iter;
					if (!Selection.GetSelected (out iter))
						return null;
					return (TemplateItem) templateStore.GetValue (iter, 2);
				}
				set {
					Gtk.TreeIter iter;
					if (templateStore.GetIterFirst (out iter)) {
						do {
							TemplateItem t = (TemplateItem) templateStore.GetValue (iter, 2);
							if (t == value) {
								Selection.SelectIter (iter);
								return;
							}
						} while (templateStore.IterNext (ref iter));
					}
				}
			}
			
			public void Add (TemplateItem templateItem)
			{
				string name = GLib.Markup.EscapeText (GettextCatalog.GetString (templateItem.Name));
				if (!string.IsNullOrEmpty (templateItem.Language))
					name += "\n<span foreground='darkgrey'><small>" + templateItem.Language + "</small></span>";
				templateStore.AppendValues (templateItem.Template.Icon.IsNull ? "md-project" : templateItem.Template.Icon.ToString (), name, templateItem);
			}
			
			public void Clear ()
			{
				templateStore.Clear ();
			}
		}
	}
}
