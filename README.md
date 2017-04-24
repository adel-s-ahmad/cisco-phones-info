## cisco IP phones information

cisco-phones-info is used to Retrieve information about cisco ip phones that registered to a UCM (Unified Communication Manager). 
Retrieved Information such as phone name, description, IP address, firmware version and any information included in the phone information web page.

## Prerequisites

1. The windows user must have write permission on the application folder to enable logging
2. Add UCM information from application's main menu choose file -> CUCM Settings
   and fill all the fields in this page which are all required except the Device Information field<br />
    a. CUCM IP/Host: enter the IP address or host name of the UCM that you want to get its registered phones,
        if you have multiple publisher UCMs then add their IP addresses in a comma separated format<br />
    b. AXL User ID: username of application user with 'Standard AXL API Access' role<br />
    c. AXL Password: password of the application user<br />
    d. Port. port number of the AXL service (the default is 8443 which will also work in most cases)<br />
    e. Version: the UCM DB schema version, you can check this from [here](https://developer.cisco.com/media/manager-xml-developer-guide/index.html?versioning.html)<br />
    f. Device Information: you can add here extra fields that you want to be retrieved from the IP phone's Device Information page, such as phone's firmware version, serial number, model number, etc<br />
        you can check available fields by visiting http://ip-phone-ip/DeviceInformationX<br />
        
