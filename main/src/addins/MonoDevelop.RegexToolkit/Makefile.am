
ADDIN_BUILD = $(top_builddir)/build/AddIns/MonoDevelop.RegexToolkit
ASSEMBLY = $(ADDIN_BUILD)/MonoDevelop.RegexToolkit.dll

DEPS =  \
	$(top_builddir)/build/AddIns/MonoDevelop.DesignerSupport/MonoDevelop.DesignerSupport.dll \
	$(top_builddir)/build/bin/MonoDevelop.Core.dll \
	$(top_builddir)/build/bin/MonoDevelop.Ide.dll

REFS =  \
	$(GLIB_SHARP_LIBS) \
	$(GTK_SHARP_LIBS) \
	$(MONO_ADDINS_LIBS) \
	-r:Mono.Posix \
	-r:System \
	-r:System.Data \
	-r:System.Web \
	-r:System.Web.Services \
	-r:System.Xml
	
FILES =  \
	AssemblyInfo.cs \
	gtk-gui/generated.cs \
	gtk-gui/MonoDevelop.RegexToolkit.RegexLibraryWindow.cs \
	gtk-gui/MonoDevelop.RegexToolkit.RegexToolkitWindow.cs \
	MonoDevelop.RegexToolkit/Commands.cs \
	MonoDevelop.RegexToolkit/RegexLibraryWindow.cs \
	MonoDevelop.RegexToolkit/RegexToolkitWindow.cs \
	MonoDevelop.RegexToolkit/Webservices.cs

RES =  \
	gtk-gui/gui.stetic \
	MonoDevelop.RegexToolkit.addin.xml \
	RegexElements.xml 



all: $(ASSEMBLY) $(ASSEMBLY).mdb

$(ASSEMBLY): $(build_sources) $(build_resources) $(DEPS)
	mkdir -p $(ADDIN_BUILD)
	$(CSC) $(CSC_FLAGS) -debug -out:$@ -target:library $(REFS) $(build_deps) \
		$(build_resources:%=/resource:%) $(build_sources)

$(ASSEMBLY).mdb: $(ASSEMBLY)

assemblydir = $(MD_ADDIN_DIR)/MonoDevelop.RegexToolkit
assembly_DATA = $(ASSEMBLY) $(ASSEMBLY).mdb

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb

EXTRA_DIST = $(FILES) $(RES)

include $(top_srcdir)/Makefile.include

