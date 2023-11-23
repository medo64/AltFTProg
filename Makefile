.PHONY: all clean release package preview test

ifeq ($(PREFIX),)
    PREFIX := /usr/local/
endif


DIST_NAME := ftprog2
DIST_VERSION := $(shell cat src/AltFTProg/AltFTProg.csproj | grep "<Version>" | sed 's^</\?Version>^^g' | xargs)

MAN_DATE := $(shell date +'%d %b %Y')

DEB_BUILD_ARCH := $(shell getconf LONG_BIT | sed "s/32/i386/" | sed "s/64/amd64/")


HAS_X11EXTRAS := $(shell test -d /usr/share/doc/libqt5x11extras5-dev ; echo $$?)
HAS_DPKGDEB := $(shell command -v dpkg-deb >/dev/null 2>&1 ; echo $$?)
HAS_LINTIAN := $(shell which lintian >/dev/null ; echo $$?)
HAS_UNCOMMITTED := $(shell git diff --quiet ; echo $$?)


all: release

clean:
	@dotnet clean ./src/AltFTProg/

release:
	@dotnet publish ./src/AltFTProg/ --configuration Release --output ./bin --self-contained true --use-current-runtime -p:PublishReadyToRun=true -p:PublishSingleFile=true -p:PublishTrimmed=true

package: clean release
	$(if $(findstring 0,$(HAS_DPKGDEB)),,$(error Package 'dpkg-deb' not installed))
	$(if $(findstring 0,$(HAS_LINTIAN)),,$(warning No 'lintian' in path, consider installing 'lintian' package))
	$(if $(findstring 0,$(HAS_UNCOMMITTED)),,$(warning Uncommitted changes present))
	@mkdir -p dist/
	@echo "Packaging for $(DEB_BUILD_ARCH)"
	@$(eval PACKAGE_NAME = $(DIST_NAME)_$(DIST_VERSION)_$(DEB_BUILD_ARCH))
	@$(eval PACKAGE_DIR = /tmp/$(PACKAGE_NAME)/)
	-@$(RM) -r $(PACKAGE_DIR)/
	@mkdir $(PACKAGE_DIR)/
	@cp -r package/deb/DEBIAN $(PACKAGE_DIR)/
	@sed -i "s/MAJOR.MINOR.PATCH/$(DIST_VERSION)/" $(PACKAGE_DIR)/DEBIAN/control
	@sed -i "s/ARCHITECTURE/$(DEB_BUILD_ARCH)/" $(PACKAGE_DIR)/DEBIAN/control
	@mkdir -p $(PACKAGE_DIR)/usr/share/doc/ftprog2/
	@cp package/deb/copyright $(PACKAGE_DIR)/usr/share/doc/ftprog2/copyright
	@mkdir -p build/man/
	@sed 's/MAJOR.MINOR.PATCH/$(DIST_VERSION)/g' docs/man/ftprog2.1 > build/man/ftprog2.1
	@sed -i 's/CURR_DATE/$(MAN_DATE)/g' build/man/ftprog2.1
	@mkdir -p $(PACKAGE_DIR)/usr/share/man/man1/
	@gzip -c9n build/man/ftprog2.1 > $(PACKAGE_DIR)/usr/share/man/man1/ftprog2.1.gz
	@find $(PACKAGE_DIR)/ -type d -exec chmod 755 {} +
	@find $(PACKAGE_DIR)/ -type f -exec chmod 644 {} +
	@chmod 755 $(PACKAGE_DIR)/DEBIAN/config $(PACKAGE_DIR)/DEBIAN/p*inst $(PACKAGE_DIR)/DEBIAN/p*rm
	@install -d $(PACKAGE_DIR)/opt/ftprog2/
	@install bin/ftprog2 $(PACKAGE_DIR)/opt/ftprog2/
	@install -m 644 bin/ftprog2.pdb $(PACKAGE_DIR)/opt/ftprog2/
	@fakeroot dpkg-deb -Z gzip --build $(PACKAGE_DIR)/ > /dev/null
	@cp /tmp/$(PACKAGE_NAME).deb dist/
	@$(RM) -r $(PACKAGE_DIR)/
	-@lintian --suppress-tags dir-or-file-in-opt,no-changelog,unstripped-binary-or-object dist/$(PACKAGE_NAME).deb
	@echo Output at dist/$(PACKAGE_NAME).deb

preview: docs/man/ftprog2.1
	@mkdir -p build/man/
	@sed 's/MAJOR.MINOR.PATCH/$(DIST_VERSION)/g' docs/man/ftprog2.1 > build/man/ftprog2.1
	@sed -i 's/CURR_DATE/$(MAN_DATE)/g' build/man/ftprog2.1
	@man -l build/man/ftprog2.1
