# ZPrinterConfig

This is a utility program used to configure Backup and Void parameters within the ZT610 and ZT620 Zebra printers. Custom Backup and Void firmware for the Zebra printer is required.

## Printer Connection Details

![](https://github.com/ZeroxCorbin/ZPrinterConfig/blob/master/ZPrinterConfig/Assets/HelpImages/PrinterConnectionDetails.png)

1.  Enter the IP address or hostname of the Zebra printer.
    1.  The printer's current IP address is displayed on the printer's home screen. It is the same IP used for the web interface.
2.  Enter the IP port number the printer uses for IP communications.
    1.  The default port number is 6101. The alternate is 9100. Both ports can be displayed and changed on the printer's interface under the network menu.

## Label Type Recommendations Pulldown
![](https://github.com/ZeroxCorbin/ZPrinterConfig/blob/master/ZPrinterConfig/Assets/HelpImages/LabelTypeMenu.png)

1.  Select the type of label you are using for Backup and Void to see recommended paramater values.
    1.  Select "Backup/Void Transfer" when using thermal transfer labels.
    2.  Select "Backup/Void Direct" when using direct thermal labels.
    3.  Select "Normal" to disable Backup and Void.
