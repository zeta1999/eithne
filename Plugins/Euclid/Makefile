MCS = mcs

TARGET = Euclid.dll

SRC = \
	Euclid.cs

all: ../$(TARGET)

../$(TARGET): ../../IPlugin.dll $(SRC)
	$(MCS) $(SRC) -out:../$(TARGET) -r:IPlugin -r:Mono.Posix -lib:../.. -target:library -pkg:gtk-sharp-2.0 -pkg:glade-sharp-2.0 -debug

clean:
	rm -f ../$(TARGET)
	rm -f ../$(TARGET).mdb
