MCS = mcs

TARGET = ../IPlugin.dll

IPLUGIN = \
	BPP.cs \
	CommSocket.cs \
	Config.cs \
	DialogMessage.cs \
	DialogQuestion.cs \
	GConfConfig.cs \
	IBlock.cs \
	ICommImage.cs \
	ICommResult.cs \
	IConfig.cs \
	IFactory.cs \
	IImage.cs \
	IInfo.cs \
	IResult.cs \
	IType.cs \
	PluginException.cs \
	Program.cs \
	RegistryConfig.cs \
	ResultSorter.cs \
	Utility.cs \
	Plugin/Base.cs \
	Plugin/In.cs \
	Plugin/Out.cs

RESOURCES = \
	DialogMessage.glade \
	DialogQuestion.glade

RESFILES = $(addprefix resources/,$(RESOURCES))
RESCMD = $(addprefix -resource:,$(RESFILES))

all: $(TARGET)

$(TARGET): $(IPLUGIN) $(RESFILES)
	$(MCS) $(IPLUGIN) $(RESCMD) -out:$(TARGET) -target:library -r:Mono.Posix -unsafe -debug -pkg:gconf-sharp-2.0 -pkg:gtk-sharp-2.0 -pkg:glade-sharp-2.0

clean:
	rm -f $(TARGET) $(TARGET).mdb
