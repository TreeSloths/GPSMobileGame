GeoArcMap - exemple application for Unity<br>
-----------------------------------------<br>
<br>
SUMMARY<br>
-------<br>
	I   - Description<br>
	II  - Database<br>
	III - Server setup<br>
	IV  - Client setup<br>
<br>
<br>
I - Description:<br>
----------------<br>
GeoArcMap is an exemple application that demonstrate a possible implementation of a map viewer allowing the creation of content stored in a remote database.<br>
In the event where the user did not setup a database, he still have a functional map viewer but will not be able to create or store maps content.<br>
The user can select at run time a provider for the maps imagery and, through scripting, add additional providers.<br>
By default the application allow to select two providers at run time: Google or ESRI.<br>
The utilisation of Google and ESRI imagery is subject to some terms and conditions and the user should make sure to read them before shiping his application<br>
for commercial profit.<br>
<br>
II - Database:<br>
--------------<br>
The user is responsible for maintaining his own database.<br>
You can find free hosts for your databases on the internet like AlwaysData.net among many others.<br>
The files needed to setup the GeoArcMap database are provided with the package in a zip: ServerFiles.zip.<br>
Because they must contain your server admin user name and password, you will have to edit two files before uploading them on a server as decribed in "III - Server setup".<br>
<br>
III - Server setup:<br>
-------------------<br>
To setup your database i will use the free hosting service provider AlwaysData.net as an exemple:<br>
1) Edit your local file "ServerFiles\Database\cgi-bin\php5.ini" and replace "user" at the end of the file in "auto_prepend_file = /home/<user>/www/db_connection.php" with the user name you created at AlwaysData.net<br>
2) Edit your local file "ServerFiles\Database\www\db_connection.php" and replace the occurences of "user" and "password" with the user name and password you created at AlwaysData.net<br>
3) Open your server directory with an application like FileZilla<br>
4) Copy the content of the "ServerFiles\Database\www" folder into the corresponding "www" remote folder<br>
5) Copy the file "ServerFiles\Database\cgi-bin\php5.ini" folder into the corresponding "cgi-bin" remote folder<br>
<br>
IV - Client setup:<br>
------------------<br>
1) Launch the application<br>
2) Open the options menu<br>
3) In the "Server" field, enter the adress of the server ( ex: "<your-application>.alwaysdata.net" )<br>
4) Save the options by clicking "OK"<br>

 
