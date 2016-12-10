/*
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

       Original work Copyright Duncan Grieve, August 2008
       Modified work Copyright 2012 Tor Lye
*/
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace mpcstats
{
    public class LCDSmartie
    {
        #region "definitions and initilisation"
        // define our variables

        string[] file_types; //file types to parse out
        string[] other_cruft; //other cruft to parse out
        string URL = ""; //where the data is
        string[] output_strings = new string[15]; //array of parsed strings to output
        string input_string = ""; // the status string from mpc
        int run_time; //for debuging/performance testing


        //start a background thread to gather and parse the data
        Thread bg_t;// = new Thread(new ThreadStart(main));
        //string dummy_init = Init(); //trick the dll into running an init sub


        public LCDSmartie()
        {
            try
            {
                set_values();

                bg_t = new Thread(new ThreadStart(main));
                bg_t.Start();
            }
            catch
            {
                //Swallow all exceptions to prevent LCDSmartie from crashing
            }
        }
        #endregion

        #region "LcdSmartie functions"

        public string function1(string param1, string param2)
        {
            //status

            switch (param1)
            {
                case "1":
                    return output_strings[5]; //status as text playing paused etc
                case "2":
                    return output_strings[6]; //status as a number
                case "3":
                    return output_strings[0]; //if mpc is open return 1 if closed return 0
                default:
                    return "valid parameters are 1, 2, or 3";
            }
        }

        public string function2(string param1, string param2)
        {
            //title

            switch (param1)
            {
                case "1":
                    return output_strings[1];
                case "2":
                    return output_strings[2];
                case "3":
                    return output_strings[3];
                case "4":
                    return output_strings[4];
                default:
                    return "valid parameters are 1, 2, 3, or 4";
            }
        }

        public string function3(string param1, string param2)
        {
            //pos and length
            switch (param1)
            {
                case "1":
                    return output_strings[7];
                case "2":
                    return output_strings[8];
                case "3":
                    return output_strings[9];
                case "4":
                    return output_strings[10];
                case "5":
                    return output_strings[11];
                case "6":
                    return output_strings[14];
                default:
                    return "valid parameters are 1, 2, 3, 4, 5, or 6";
            }
        }

        public string function4(string param1, string param2)
        {
            //volume
            switch (param1)
            {
                case "1":
                    return output_strings[12];
                case "2":
                    return output_strings[13];
                default:
                    return "valid parameters are 1 or 2";
            }
        }

        public string function5(string param1, string param2)
        {
            //diagnostics  
            switch (param1)
            {
                case "1":
                    return URL;
                case "2":
                    return input_string;
                case "3":
                    return Convert.ToString(run_time);
                default:
                    return "valid parameters are 1, 2 or 3";
            }
        }

        public string function20(string param1, string param2)
        {
            //credits
            return " Written by Duncan Grieve, duncan.grieve@gmail.com";
        }

        public int GetMinRefreshInterval()
        {
            //Define the minimum interval that a screen should get fresh data from our plugin.
            // The actual value used by Smartie will be the higher of this value and of the "dll check interval" setting
            // on the Misc tab.  [This function is optional, Smartie will assume 300ms if it is not provided.]
            //
            return 250; // 300 ms (around 3 times a second)
        }

        #endregion

        #region "Main functions"

        private static string[] status_string_to_array(string info)
        {
            if (info.StartsWith("OnStatus('"))
                return status_string_to_array_singleQuote(info);
            else
                return status_string_to_array_doubleQuote(info);
        }

        /// <summary>
        /// Parse the new single quote pattern in MPC-HC 1.5.2 and newer.
        /// E.g. OnStatus("Media Player Classic Home Cinema", "N/A", 0, "00:00:00", 0, "00:00:00", 0, 95, "")
        /// </summary>
        private static string[] status_string_to_array_doubleQuote(string info)
        {
            string[] dataarray = new string[8]; // ends up containing the parsed info

            Regex regex = new Regex(@"OnStatus\(""([^""]+)"", ""([^""]+)"", (\d+), ""([^""]+)"", (\d+), ""([^""]+)"", (\d+), (\d+)");
            MatchCollection matches = regex.Matches(info);
            Match match = matches[0];

            for (int i = 0; i < 8; i++)
            {
                dataarray[i] = match.Groups[i+1].Value;
            }

            // Remove escaped single quotes in title
            dataarray[0] = dataarray[0].Replace("\\'", "'");
            
            // now remove the mpc part from the title and return the file name or "no file loded"
            dataarray[0] = remove_mpc_title(dataarray[0]);

            // now we have an array of strings with only theperetinent information from the status string
            // the array is now ready for further parsing 
            // all of the repetative work has been done on all the elemets
            // now to work on the individual array elements

            return dataarray;
        }

        /// <summary>
        /// Parse the old style single quote pattern in MPC-HC 1.5.1 and older.
        /// E.g. OnStatus('Media Player Classic Home Cinema', 'n/a', 0, '00:00:00', 0, '00:00:00', 0, 95)
        /// </summary>
        private static string[] status_string_to_array_singleQuote(string info)
        {
            string[] dataarray = null;
            // ends up containing the parsed info
            int i = 0;
            int j = 0;
            //Now parse the string in to the 8 diffrent pieces of information removing all the useless cruft
            // and out put as an array

            info = info.Substring(9);
            // first remove the leading usless characters( onstatus(  )
            info = StrReverse(info);
            // reverse the order of the string to place the usless chars that were at the end at the start
            info = info.Substring(1);
            //remove the useless chars from the 'end' of the string

            dataarray = info.Split(',');
            //split the info string everytime it finds a ","

            //if there is a "," in the title string this sorts it out
            if (dataarray.GetUpperBound(0) > 7)
            {
                for (i = 7; i <= (dataarray.GetUpperBound(0) - 1); i++)
                {
                    if (i != 7)
                    {
                        dataarray[7] = dataarray[7] + "," + dataarray[i];
                        // rebuild the title string
                    }
                }
            }

            Array.Resize(ref dataarray, 8);
            // remove the unneeded values preserving the existing ones and dimension the array 

            Array.Reverse(dataarray);
            //flip the array order so that it matches the input

            for (j = 0; j <= 7; j++)
            {
                dataarray[j] = StrReverse(dataarray[j]);
                // flip the indiviual elements around
                dataarray[j] = dataarray[j].Trim();
                // remove white space
                if (dataarray[j].StartsWith("'") && dataarray[j].EndsWith("'"))
                {
                    dataarray[j] = dataarray[j].Substring(1, dataarray[j].Length - 2);
                    //remove the leading and trailing quotes
                }
            }
            // now remove the mpc part from the title and return the file name or "no file loded"
            dataarray[0] = remove_mpc_title(dataarray[0]);

            // now we have an array of strings with only theperetinent information from the status string
            // the array is now ready for further parsing 
            //all of the repetative work has been done on all the elemets
            // now to work on the individual array elements

            return dataarray;
        }

        private static string remove_mpc_title(string input)
        {
            //this function takes a string and removes the "media playr classic ... bit from the title

            if (input.StartsWith("Media Player Classic"))
            {
                string[] components = input.Split(new char[] { '-' }, 2);
                if (components.Length == 2)
                    return components[1];
            }

            //Console.WriteLine("title_out  " & title_out)
            if (input.StartsWith("Media Player Classic"))
            {
                return "No File Open";
            }

            return input;
        }

        private static string get_status_string(string url)
        {
            //this function gets the status string from mpc
            //must catch exception here
            string outString = "";

            try
            {
                WebClient client = new WebClient();
                Stream data = client.OpenRead(url);
                StreamReader reader = new StreamReader(data);
                outString = reader.ReadLine();
                // get the string from the player 
            }
            catch
            {
                //Ignore all exceptions
            }
            return outString;
        }

        private static string rm_punct_spaces(string input)
        {
            string output = null;
            //remove the unneeded punctuation characters and replace them with white space

            output = input.Replace("_", " ");
            output = output.Replace("-", " ");
            output = output.Replace(".", " ");
            output = output.Replace("(", " ");
            output = output.Replace(")", " ");
            output = output.Replace("[", " ");
            output = output.Replace("]", " ");
            output = output.Replace("}", " ");
            output = output.Replace("{", " ");

            // the above could result in multiple white spaces together so we shall check 
            // for them and then exterminate
            do
            {
                output = output.Replace("  ", " "); //exterminate
            } while (!(output.Contains("  ") == false));

            output = output.Trim(); // remove any leading or trailing spaces

            return output;
        }

        private static string find_remove(string input, string[] search_array)
        {
            int temp_pos = 0;
            int rm_pos = input.Length + 2;
            string output = null;
            int i = 0;
            //set the input string up for searching
            input = rm_punct_spaces(input);

            input = " " + input + " "; //pad so stuff on the ends gets caught

            for (i = 0; i <= (search_array.Length - 1); i++)
            {
                temp_pos = input.IndexOf(" " + search_array[i] + " ", StringComparison.InvariantCultureIgnoreCase) + 1;

                if (temp_pos != 0 && temp_pos < rm_pos)
                {
                    rm_pos = temp_pos;
                }
            }

            output = (rm_pos < input.Length) ? input.Substring(0, rm_pos) : input;
            output = output.Trim();
            return output;
        }

        private static string status_as_number(string input)
        {

            switch (input)
            {
                case "Closed":
                    return "0";
                case "Playing":
                    return "1";
                case "Paused":
                    return "2";
                case "Stopped":
                    return "3";
                case "Opening...":
                    return "4";
                default:
                    return "0";
            }
        }

        private string remove_cruft(string input, bool remove_all)
        {
            string output = null;

            //check there is a file actualy open
            if (input == "No File Open")
            {
                return input;
            }

            //remove the file type
            output = find_remove(input, file_types);

            //If asked remove the scene stuff as well
            if (remove_all == true)
            {
                //strip out the scene stuff
                output = find_remove(output, other_cruft);
            }

            //if the output is very short there's probably been a problem just 
            //spit back out the original title minus spaces etc
            if (output.Length < 4)
            {
                return rm_punct_spaces(input);
            }

            return output;
        }

        private static string percentage(string position, string length)
        {
            string output = null;
            double len_int = Double.Parse(length);
            double pos_int = Double.Parse(position);
            double tmp = 0;


            if (len_int == 0 || pos_int == 0)
            {
                return "0";
            }

            tmp = (pos_int / len_int) * 100; // fix for divide by zero

            output = Convert.ToString(tmp);
            return output;
        }

        private static string status_as_string(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "closed";
            }

            return input;
        }

        private static string time_var(string input)
        {

            if (input != "00:00:00")
            {
                while (input.StartsWith("0") || input.StartsWith(":"))
                {
                    input = input.Remove(0, 1);
                }

                return input;
            }
            else
            {
                return input;
            }
        }

        private static string remaining_time(string total_str, string position_str)
        {
            string[] total_array_str = total_str.Split(':');
            string[] pos_array_str = position_str.Split(':');
            int[] total_array = new int[3];
            int[] pos_array = new int[3];
            string[] out_array = new string[3];
            int total_sec = 0;
            int pos_sec = 0;
            int remaing_sec = 0;
            int i = 0;


            // convert to int for processing
            for (i = 0; i <= 2; i++)
            {
                total_array[i] = Convert.ToInt32(total_array_str[i]);
                pos_array[i] = Convert.ToInt32(pos_array_str[i]);
            }


            //work out the total number of seconds and seconds passed
            total_sec = total_array[0] * 3600 + total_array[1] * 60 + total_array[2];
            pos_sec = pos_array[0] * 3600 + pos_array[1] * 60 + pos_array[2];

            remaing_sec = total_sec - pos_sec;

            out_array[0] = Convert.ToString(remaing_sec / 3600); //hrs

            out_array[1] = Convert.ToString((remaing_sec % 3600) / 60); //minutes

            out_array[2] = Convert.ToString(remaing_sec % 60); //seconds

            if (out_array[1].Length == 1)
            {
                out_array[1] = "0" + out_array[1];
            }

            if (out_array[2].Length == 1)
            {
                out_array[2] = "0" + out_array[2];
            }

            if (total_array[0] == 0)
            {
                return out_array[1] + ":" + out_array[2];
            }

            return out_array[0] + ":" + out_array[1] + ":" + out_array[2];
        }

        public void set_values()
        {
            //this reads in all the values from the config file
            URL = My.MySettings.Default.url;

            int len_sets = My.MySettings.Default.cruft.Count;

            int len_files = My.MySettings.Default.file_types.Count;

            other_cruft = new string[len_sets + 1];
            file_types = new string[len_files + 1];

            My.MySettings.Default.cruft.CopyTo(other_cruft, 0);

            My.MySettings.Default.file_types.CopyTo(file_types, 0);
        }


        private void process_title()
        {
            string[] dataarray = null; // ends up containing the parsed info from the status string

            dataarray = status_string_to_array(input_string); //parse the string into the dataarray

            //title parsing
            output_strings[1] = dataarray[0];
            output_strings[2] = rm_punct_spaces(dataarray[0]); //no punctuation
            output_strings[3] = remove_cruft(dataarray[0], false); //no file extention
            output_strings[4] = remove_cruft(dataarray[0], true); //no scene cruft

            //status parsing
            output_strings[5] = status_as_string(dataarray[1]);
            output_strings[6] = status_as_number(dataarray[1]);

            //length fixed and variable
            output_strings[7] = dataarray[5]; //fixed length
            output_strings[8] = time_var(dataarray[5]);

            //position fixed and variable
            output_strings[9] = dataarray[3]; //fixed length
            output_strings[10] = dataarray[3].Remove(0,
                dataarray[3].Length - output_strings[8].Length);

            //percent played
            output_strings[11] = percentage(dataarray[2], dataarray[4]);

            //volume
            output_strings[12] = dataarray[7];

            //mute status'
            output_strings[13] = dataarray[6];

            //remaining time
            output_strings[14] = remaining_time(dataarray[5], dataarray[3]);
        }

        public void main()
        {
            int end_time = 0;
            int start_run = 0;

            while (true)
            {
                start_run = Environment.TickCount;
                end_time = Environment.TickCount + 250;

                input_string = get_status_string(URL);
                //call the function to get the status string

                if (string.IsNullOrEmpty(input_string))
                {
                    output_strings[0] = "0";
                    //clear the others mabey?
                    output_strings[1] = "Mpc closed";
                    output_strings[2] = "Mpc closed";
                    output_strings[3] = "Mpc closed";
                    output_strings[4] = "Mpc closed";
                    output_strings[5] = "Mpc closed";
                    output_strings[6] = "0";
                    output_strings[7] = "00:00:00";
                    output_strings[8] = "00:00:00";
                    output_strings[9] = "00:00:00";
                    output_strings[10] = "00:00:00";
                    output_strings[11] = "0";
                    output_strings[12] = "0";
                }
                else
                {
                    //to do move the individual local variables to the global output array
                    // and split this section in to 2 subroutines

                    output_strings[0] = "1"; //mpc is running cause we have meaningfull data

                    process_title();

                }

                run_time = Environment.TickCount - start_run;

                while (!(Environment.TickCount > end_time))
                {
                    Thread.Sleep(5);
                }
            }
        }

        #endregion

        /// <summary>
        /// Returns a string in which the character order of a specified string
        /// is reversed. Replaces Visual Basic Strings.StrReverse method.
        /// </summary>
        private static string StrReverse(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            char[] charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
