Alternate FT Prog
=================

Linux alternative to FT Prog utility.


## Usage

Run without arguments to list all devices:

    altftprog


To apply template (made by FT_Prog) just give file as an argument:

    altftprog template.xml


To get more details, add `-v` parameter:

    altftprog -v


## Setup

### Install Libraries

This application requires `libftdi` library. On Ubuntu you can install it using:
```bash
sudo apt install libftdi-dev
```

### Setup User Permissions

You might want to setup your user permissions and setup UDEV rules:
```bash
sudo usermod -aG dialout $USER
sudo usermod -aG tty $USER
echo 'ACTION=="add", SUBSYSTEM=="usb", ATTRS{idVendor}=="0403", ATTRS={idProduct}=="6001", OWNER="user", MODE="0777", GROUP="dialout"' | tee /etc/udev/rules.d/99-libftdi.rules
```
### Issues

After enumeration, FTDI devices will lose their serial port (e.g. `ttyUSB0`).
