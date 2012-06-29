# Makefile for compiling Noda Time under mono.
# See tools/userguide-src/markdown/mono.txt for requirements.

# Assumes that 'mono', 'xbuild' and 'nunit-console' point to appropriate
# versions of the respective tools. If this is not true, override the
# two assignments, either by editing the below, or by running 'make XBUILD=...'

MONO := mono
XBUILD := xbuild
NUNIT := nunit-console
# For example, to use a version of NUnit that has been unzipped somewhere else,
# use something like the following instead.
# NUNIT := mono ../NUnit-2.6.0.12051/bin/nunit-console.exe

# Targets:
#   debug (default)
#     builds everything (including tests, benchmarks, demo code, the zoneinfo
#     compiler, etc) in debug configuration
#   release
#     builds everything in release configuration, including the XML
#     documentation (which is not built by default)
#   check
#     runs the tests under NUnit.
#   clean
#     runs the Clean target for all projects, removing the immediate output
#     from each.  Note that this does not remove _all_ generated files. 
#
#   userguide
#     builds the Markdown-based documentation generator, and updates the
#     userguide documentation (in docs/userguide).

XBUILDFLAGS := /p:TargetFrameworkVersion='v3.5' /p:TargetFrameworkProfile=''
XBUILDFLAGS_DEBUG := $(XBUILDFLAGS)
XBUILDFLAGS_RELEASE := $(XBUILDFLAGS) /p:Configuration=Release

SOLUTION := 'src/NodaTime Mono.sln'
TOOLS_SOLUTION := 'tools/NodaTime.Tools.sln'
DEBUG_TEST_DLL := src/NodaTime.Test/bin/Debug/NodaTime.Test.dll
MARKDOWN_TOOL := tools/NodaTime.Tools.BuildMarkdownDocs/bin/Release/NodaTime.Tools.BuildMarkdownDocs.exe

debug:
	$(XBUILD) $(XBUILDFLAGS_DEBUG) $(SOLUTION)

release:
	$(XBUILD) $(XBUILDFLAGS_RELEASE) $(SOLUTION)

check: debug
	$(NUNIT) $(DEBUG_TEST_DLL)

tools:
	$(XBUILD) $(XBUILDFLAGS_RELEASE) $(TOOLS_SOLUTION)

userguide: tools
	$(MONO) $(MARKDOWN_TOOL) tools/userguide-src docs/userguide

clean:
	$(XBUILD) $(XBUILDFLAGS_DEBUG) $(SOLUTION) /t:Clean
	$(XBUILD) $(XBUILDFLAGS_DEBUG) $(TOOLS_SOLUTION) /t:Clean
	$(XBUILD) $(XBUILDFLAGS_RELEASE) $(SOLUTION) /t:Clean
	$(XBUILD) $(XBUILDFLAGS_RELEASE) $(TOOLS_SOLUTION) /t:Clean

.SUFFIXES:
.PHONY: debug release check tools userguide clean
