
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug "-define:DEBUG"
ASSEMBLY = ../build/PyBinding.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ../build

PYBINDING_DLL_MDB_SOURCE=bin/Debug/PyBinding.dll.mdb
PYBINDING_DLL_MDB=$(BUILD_DIR)/PyBinding.dll.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+
ASSEMBLY = ../build/PyBinding.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ../build

PYBINDING_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=.resources.dll

PROGRAMFILES = \
	$(PYBINDING_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(PYBINDING_PC)  


RESGEN=resgen2

INSTALL_DIR = $(DESTDIR)$(prefix)/lib/monodevelop/AddIns/PyBinding

PYBINDING_PC = $(BUILD_DIR)/monodevelop-pybinding.pc

FILES =  \
	AssemblyInfo.cs \
	gtk-gui/generated.cs \
	gtk-gui/PyBinding.Gui.PythonOptionsWidget.cs \
	PyBinding.Compiler/IPythonCompiler.cs \
	PyBinding.Compiler/Python25Compiler.cs \
	PyBinding.Gui.Navigation/AttributeNodeBuilder.cs \
	PyBinding.Gui.Navigation/ClassNodeBuilder.cs \
	PyBinding.Gui.Navigation/FunctionNodeBuilder.cs \
	PyBinding.Gui.Navigation/PackageNodeBuilder.cs \
	PyBinding.Gui.Navigation/PackagesNodeBuilder.cs \
	PyBinding.Gui.Navigation/ProjectNodeBuilderExtension.cs \
	PyBinding.Gui/PythonEditorCompletion.cs \
	PyBinding.Gui/PythonEditorIndentation.cs \
	PyBinding.Gui/PythonEditorOutline.cs \
	PyBinding.Gui/PythonOptionsPanel.cs \
	PyBinding.Gui/PythonOptionsWidget.cs \
	PyBinding.Parser.Dom/PythonArgument.cs \
	PyBinding.Parser.Dom/PythonAttribute.cs \
	PyBinding.Parser.Dom/PythonClass.cs \
	PyBinding.Parser.Dom/PythonComment.cs \
	PyBinding.Parser.Dom/PythonCompilationUnit.cs \
	PyBinding.Parser.Dom/PythonFunction.cs \
	PyBinding.Parser.Dom/PythonImport.cs \
	PyBinding.Parser.Dom/PythonLocal.cs \
	PyBinding.Parser.Dom/PythonModule.cs \
	PyBinding.Parser.Dom/PythonNode.cs \
	PyBinding.Parser.Dom/PythonParsedDocument.cs \
	PyBinding.Parser/ParserDatabase.cs \
	PyBinding.Parser/ParserItem.cs \
	PyBinding.Parser/ParserManager.cs \
	PyBinding.Parser/PythonExpressionFinder.cs \
	PyBinding.Parser/PythonParser.cs \
	PyBinding.Parser/PythonParserInternal.cs \
	PyBinding.Parser/PythonResolver.cs \
	PyBinding.Runtime/AbstractPythonRuntime.cs \
	PyBinding.Runtime/IPythonRuntime.cs \
	PyBinding.Runtime/IronPythonRuntime.cs \
	PyBinding.Runtime/Python25Runtime.cs \
	PyBinding.Runtime/Python26Runtime.cs \
	PyBinding.Runtime/Python27Runtime.cs \
	PyBinding/IronPythonExecutionHandler.cs \
	PyBinding/PythonConfiguration.cs \
	PyBinding/PythonExecutionCommand.cs \
	PyBinding/PythonExecutionHandler.cs \
	PyBinding/PythonHelper.cs \
	PyBinding/PythonLanguageBinding.cs \
	PyBinding/PythonProject.cs \
	PyBinding/PythonProjectBinding.cs \
	PyBinding/PythonSite.cs 

DATA_FILES = 

RESOURCES =  \
	gtk-gui/gui.stetic \
	PyBinding.addin.xml \
	Resources/BasicScriptPySourceFile.xft.xml \
	Resources/completion.py \
	Resources/EmptyDjangoProject.xpt.xml \
	Resources/EmptyPyProject.xpt.xml \
	Resources/EmptyPySourceFile.xft.xml \
	Resources/PyClutterProject.xpt.xml \
	Resources/PyGtkProject.xpt.xml \
	Resources/py-icon-32.png \
	Resources/PythonStylePolicy.xml \
	Resources/Schema.sql \
	Resources/text-x-pysrc.16x16.png 

EXTRAS = \
	monodevelop-pybinding.pc.in 

REFERENCES =  \
	Mono.Data.Sqlite \
	Mono.Posix \
	-pkg:glib-sharp-2.0 \
	-pkg:gtk-sharp-2.0 \
	-pkg:mono-addins \
	-pkg:monodevelop \
	-pkg:monodevelop-core-addins \
	System \
	System.Core \
	System.Data \
	System.Xml

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-wrapper,PYBINDING_PC,monodevelop-pybinding.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'


LOCAL_PKGCONFIG=PKG_CONFIG_PATH=../../../local-config:$$PKG_CONFIG_PATH


$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	make pre-all-local-hook prefix=$(prefix)
	mkdir -p $(shell dirname $(ASSEMBLY))
	make $(CONFIG)_BeforeBuild
	$(LOCAL_PKGCONFIG) $(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
	make $(CONFIG)_AfterBuild
	make post-all-local-hook prefix=$(prefix)

install-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-install-local-hook prefix=$(prefix)
	mkdir -p $(INSTALL_DIR)
	$(call cp,$(ASSEMBLY),$(INSTALL_DIR))
	$(call cp,$(ASSEMBLY_MDB),$(INSTALL_DIR))
	mkdir -p '$(DESTDIR)$(libdir)/pkgconfig'
	$(call cp,$(PYBINDING_PC),$(DESTDIR)$(libdir)/pkgconfig)
	$(call cp,$(PYBINDING_DLL_MDB),$(INSTALL_DIR))
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(INSTALL_DIR))
	$(call rm,$(ASSEMBLY_MDB),$(INSTALL_DIR))
	$(call rm,$(PYBINDING_PC),$(DESTDIR)$(libdir)/pkgconfig)
	$(call rm,$(PYBINDING_DLL_MDB),$(INSTALL_DIR))
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
