# mpcstats
Media Player Classic HC plugin for LCD Smartie

This is a plugin for [LCD Smartie](http://lcdsmartie.sourceforge.net/), to display 
information from [MPC-HC](https://mpc-hc.org/) (Media Player Classic - Home Cinema)

This project was originally created by Duncan Grieve in 2008 and forked by myself in 2012. 
The project was originally written in VB.NET, but was converted to C# using a tool.

# Installation #
Get mpcstats.dll, mpcstats.xml, and mpcstats.dll.config from the latest release. Copy these files to the LCD Smartie plugin directory.

Make sure you have msvcr71.dll in the LCD Smartie directory. Otherwise you may get a "Load of plugin failed" error.

The .NET Framework 3.5 is required to run the plugin.

# Set up #

* Start MPC-HC and enable the web interface.
 * View -> Options -> Web Interface.
 * Check the box "Listen on port" 
 * Make sure that "13579" is in the port box
 * Click OK.

* Launch LCD Smartie and test the plugin.

# Functions #
The plugin has 6 functions called with `$dll(mpcstats,[function_number],[parameter],)` The second parameter is ignored.

## Function 1: Status ##

	Parameter 1 - returns the status of media player classic ie playing, paused, closed
	Parameter 2 - returns a number representing the status as above to use in actions 
			closed = 0, playing = 1, paused = 2, stoped = 3, opening = 4.
	Parameter 3 - returns 0 if media player classic is closed and 1 if it's open.

## Function 2: Title ##

	Parameter 1 - returns the file name no parsing done.
			tv_show - eppisode-name.s2e05.xvid.avi becomes tvshow - eppisode-name.s2e05.xvid.avi
	Parameter 2 - removes the punctuation and spaces from the file name. Helps legability on displays.
			tv_show - eppisode-name.s2e05.xvid.avi => tvshow eppisode name s2e05 xvid avi
	Parameter 3 - removes the file extention as well
			tv_show - eppisode-name.s2e05.xvid.avi => tvshow eppisode name s2e05 xvid
	Parameter 4 - removes the unneeded scene tags so:
			tv_show - eppisode-name.s2e05.xvid.avi => tvshow eppisode name s2e05


## Function 3: Position and length ##
	Parameter 1 - returns the length of the loaded file for a 45min file => 00:45:00
	Parameter 2 - same as above but will only return the needed portion 45min file => 45:00
	Parameter 3 - position in the file
	Parameter 4 - position in the file to match parameter 2
	Parameter 5 - percent of the file played to ease building a bar graph
	Parameter 6 - time remaining

## Function 4: Volume ##
	Parameter 1 - volume level 0 to 100
	Parameter 2 - mute status 0 = unmuted 1 = muted

## Function 5: Diagnostics ##
	Parameter 1 - the url the program is querying for the information.
			The default is "http://localhost:13579/status.html" media player classic does not need to be on the 
			same computer as lcdsmartie though i have not yet done through testing of this.
	Parameter 2 - the original string receievd from media player classic

## Function 6: Credits ##
	Call it with any parameter to see the credits
  
# Examples #
## Example screen for a 40x4 LCD ##

	$dll(mpcstats,2,4,) 
	$dll(mpcstats,3,4,)/$dll(mpcstats,3,2,) $dll(mpcstats,1,1,)
	volume:$dll(mpcstats,4,1,)
	$Bar($dll(mpcstats,3,5,),100,40) 

This will give the following output wile playing this file "rain_of_madness_h720p.mov":
![40x4 example](https://cloud.githubusercontent.com/assets/9637794/20895196/ba796bb8-bb18-11e6-9ffd-3e520f91b2b9.gif)

## For a 16x2 LCD ##
	$dll(mpcstats,2,1,)  
	$Right($dll(mpcstats,3,4,),$7%)  $Right($dll(mpcstats,3,2,),$7%)

![16x2 example 1](https://cloud.githubusercontent.com/assets/9637794/20895332/4f4ac246-bb19-11e6-83ed-771bc3588d27.png)

## Another 16x2 example (bar graph instead of file name) ##
	$Bar($dll(mpcstats,3,5,),100,16)
	$Right($dll(mpcstats,3,4,),$7%)  $Right($dll(mpcstats,3,2,),$7%)

![16x2 example 2](https://cloud.githubusercontent.com/assets/9637794/20895333/4f55db36-bb19-11e6-99c2-8be8f1af250c.png)

## Disable when MPC-HC is not running ##
In "Actions", add the following two rules:

If `$dll(mpcstats,1,3,)` `=` `1` Then `GotoScreen(1)`

If `$dll(mpcstats,1,3,)` `=` `0` Then `GotoScreen(2)`

This assumes 1 is the screen you want to show when MPC-HC is running, and 2 is the screen you want to show when MPC-HC is not running.
