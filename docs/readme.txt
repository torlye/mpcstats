mpcstatus version 1.2

1. To set up the plugin copy mpcstats.dll, mpcstats.xml, and mpcstats.dll.config to the lcd smartie plugin directory. 

2.start media player classic and enable the web interface 
view > options > webinterface check the box "listen on port" and make sure that "13579" is in the port box (the rest of the settings don't matter) and click ok. 

3. Launch Lcdsmartie and test the plugin.


This plugin is designed to be used with the latest version of Media Player Classic - Homecinema (v1.1.0.0 at time of writing) but it should work with any version of media player classic with a web interface.

http://mpc-hc.sourceforge.net/

The .net framework 3.5 is required to run the plugin

http://www.microsoft.com/downloads/details.aspx?familyid=333325FD-AE52-4E35-B531-508D977D32A6&displaylang=en




The plugin has 6 functions called with $dll(mpcstats,[function_number],[parameter],)  the second parameter is ignored


function 1 status

	parameter 1 - returns the status of media player classic ie playing, paused, closed
	parameter 2 - returns a number representing the status as above to use in actions 
			closed = 0, playing = 1, paused = 2, stoped = 3, opening = 4.
	parameter 3 - returns 0 if media player classic is closed and 1 if it's open.

function 2 title

	parameter 1 - returns the file name no parsing done. tv_show - eppisode-name.s2e05.xvid.avi becomes tvshow - eppisode-name.s2e05.xvid.avi
	parameter 2 - removes the punctuation and spaces from the file name. Helps legability on displays tv_show - eppisode-name.s2e05.xvid.avi => tvshow eppisode name s2e05 			xvid avi
	parameter 3 - removes the file extention as well tv_show - eppisode-name.s2e05.xvid.avi => tvshow eppisode name s2e05 xvid
	parameter 4 - removes the unneeded scene tags so tv_show - eppisode-name.s2e05.xvid.avi => tvshow eppisode name s2e05


function 3 position and length
	parameter 1 - returns the length of the loaded file for a 45min file => 00:45:00
	parameter 2 - same as above but will only return the needed portion 45min file => 45:00
	parameter 3 - position in the file
	parameter 4 - position in the file to match parameter 2
	parameter 5 - percent of the file played to ease building a bar graph
	parameter 6 - time remaining

function 4 volume
	parameter 1 - volume level 0 to 100
	parameter 2 - mute status 0 = unmuted 1 = muted

function 5 diagnostics
	parameter 1 - the url the program is querying for the information the default is "http://localhost:13579/status.html" media player classic does not need to be on the 			same computer as lcdsmartie though i have not yet done through testing of this.
	parameter 2 - the original string receievd from media player classic

function 6 credits
call it with any parameter to see the credits

so an example screen would be (for a 40x4 lcd)

line1:$dll(mpcstats,2,4,) 
line2:$dll(mpcstats,3,4,)/$dll(mpcstats,3,2,) $dll(mpcstats,1,1,)
line3:volume:$dll(mpcstats,4,1,)
line4:$Bar($dll(mpcstats,3,5,),100,40) 

will give the output wile playing this file "rain_of_madness_h720p.mov"

the gif is in the zip file

The source code is included in the download and it is all licenced under the GPL it was developed in VB.net 2008 using Microsoft Visual Basic 2008 Express Edition.

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.


Copyright Duncan Grieve, August 2008