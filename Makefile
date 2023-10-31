.PHONY: all clean distclean dist debug release package test

all: release

clean: 
	@dotnet clean ./src/AltFTProg/

release:
	@dotnet publish ./src/AltFTProg/ --configuration Release --output ./bin --self-contained true --use-current-runtime -p:PublishReadyToRun=true -p:PublishSingleFile=true -p:PublishTrimmed=true
