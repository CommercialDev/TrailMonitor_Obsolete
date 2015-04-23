using System;
using System.Collections;
using System.Threading;
using System.Net;
using System.IO;
using System.Security.Cryptography;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;
using Microsoft.SPOT.Time;

using Json.NETMF;


using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;

namespace GadgeteerCamera4
{
    public partial class Program
    {
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            characterDisplay.Clear();
            characterDisplay.CursorHome();
            characterDisplay.Print("Program Started");

            button.ButtonPressed += button_ButtonPressed;
            camera.PictureCaptured += camera_PictureCaptured;

            initializeNetwork();
            //initializeWireless();

            if (ethernetJ11D.IsNetworkUp)
            {
                Debug.Print("Connected to network");
                ListNetorkInterfaces();
                characterDisplay.Clear();
                characterDisplay.CursorHome();
                Microsoft.SPOT.Net.NetworkInformation.NetworkInterface settings = ethernetJ11D.NetworkSettings;
                DateTime time = new DateTime();
                time = setLocalTimeWebAPI();
                Microsoft.SPOT.Hardware.Utility.SetLocalTime(time);
                Debug.Print("Current Time is: " + DateTime.Now.ToString());
                
                characterDisplay.Print(DateTime.Now.ToString() + "\n");
                characterDisplay.Print("IP: " + settings.IPAddress);

            }

            

        }

        private DateTime setLocalTimeWebAPI()
        {
            try
            {
                System.Net.WebRequest request2 = System.Net.WebRequest.Create("https://trailmonitorapi.azurewebsites.net/api/getdatetime/");
                System.Net.WebResponse response2 = request2.GetResponse();
                Debug.Print("StatusDescription: " + ((System.Net.HttpWebResponse)response2).StatusDescription);
                System.IO.Stream dataStream2 = response2.GetResponseStream();
                System.IO.StreamReader reader2 = new System.IO.StreamReader(dataStream2);
                string responseFromServer2 = reader2.ReadToEnd();
                Debug.Print("responseFromServer2: " + responseFromServer2);
                reader2.Close();
                response2.Close();


                string string2find3 = "LocalDate\":\"";
                string string2find4 = "\"";
                int begin = responseFromServer2.IndexOf(string2find3) + string2find3.Length;
                int end = responseFromServer2.IndexOf(string2find4, begin);
                string datetimeX = responseFromServer2.Substring(begin, end - begin);
                Debug.Print("DatTime from WebAPI is: " + datetimeX);

                string year = datetimeX.Substring(0, 4);
                string month = datetimeX.Substring(5, 2);
                string day = datetimeX.Substring(8, 2);
                string hour = datetimeX.Substring(11, 2);
                string minute =  datetimeX.Substring(14, 2);
                string second =  datetimeX.Substring(17, 2);


                Debug.Print("Year: " + year);
                Debug.Print("Month: " + month);
                Debug.Print("Day: " + day);
                Debug.Print("Hour: " + hour);
                Debug.Print("Minute: " + minute);
                Debug.Print("Second: " + second);

                DateTime time = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(day), Int32.Parse(hour), Int32.Parse(minute), Int32.Parse(second));
                return time;
            }
            catch (System.Net.WebException wex)
            {
                Debug.Print("WebAPI Exception: " + wex.Message);
                DateTime time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                return time;
            }
            catch (Exception ex)
            {
                Debug.Print("WebAPI Exception: " + ex.Message);
                DateTime time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                return time;
            }
        }

        private void ListNetorkInterfaces()
        {
            var settings = ethernetJ11D.NetworkSettings;

            Debug.Print("---------------------------------");
            Debug.Print("MAC: " + ByteExtensions.ToHexString(settings.PhysicalAddress));
            Debug.Print("IP Address: " + settings.IPAddress);
            Debug.Print("DHCP Enabled: " + settings.IsDhcpEnabled);
            Debug.Print("Subnet Mask: " + settings.SubnetMask);
            Debug.Print("Gateway: " + settings.GatewayAddress);
            Debug.Print("---------------------------------");
        }

        private void initializeNetwork()
        {
            try
            {

                ethernetJ11D.NetworkInterface.Open();
                ethernetJ11D.NetworkSettings.EnableDhcp();
                ethernetJ11D.NetworkSettings.EnableDynamicDns();
                
                ethernetJ11D.UseStaticIP("192.168.1.222", "255.255.255.0", "192.168.1.1");
                ethernetJ11D.UseDHCP();
                Thread.Sleep(1250);

                //while (ethernetJ11D.NetworkSettings.IPAddress == "192.168.1.222")
                //{
                //    Debug.Print("Waiting for DHCP");
                //    Thread.Sleep(250);
                //}
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }

            if (ethernetJ11D.IsNetworkUp)
            {
                multicolorLED.TurnGreen();
            }
            else
            {
                multicolorLED.BlinkRepeatedly(GT.Color.Red);
            }
        }

        private void initializeWireless() 
        {
            //try
            //{
            //    wifiRS21.NetworkInterface.Open();
            //    wifiRS21.NetworkSettings.EnableDhcp();
            //    wifiRS21.NetworkSettings.EnableDynamicDns();
            //    wifiRS21.NetworkInterface.Join("HOLLIDAY305", "gray8468");
            //    wifiRS21.UseStaticIP("192.168.1.222", "255.255.255.0", "192.168.1.1");
            //    wifiRS21.UseDHCP();
            //    Thread.Sleep(1250);

            //    while (wifiRS21.NetworkSettings.IPAddress == "192.168.1.222")
            //    {
            //        Debug.Print("Waiting for DHCP");
            //        Thread.Sleep(250);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Debug.Print(ex.Message);
            //}

            //if (wifiRS21.IsNetworkUp)
            //{
            //    multicolorLED.TurnGreen();
            //}
            //else
            //{
            //    multicolorLED.BlinkRepeatedly(GT.Color.Red);
            //}

        }

        private void camera_PictureCaptured(Camera sender, GT.Picture picture)
        {
            try
            {
                Debug.Print("picture.PictureData.Length: " + picture.PictureData.Length.ToString());
                if (picture.PictureData.Length > 0)
                {
                    string newFileName = Guid.NewGuid().ToString() + ".bmp";
                    insertImageintoAzureBlob(picture, newFileName);
                    //saveImageSDCard(picture, newFileName);
                }
                else
                {
                    Debug.Print("Image not found, has a length <= 0");
                }

            }
            catch (Exception ex)
            {
                Debug.Print("Error: " + ex.Message);
                //multicolorLED.BlinkRepeatedly(GT.Color.Red);
            }
        }

        private void insertImageintoAzureBlob(GT.Picture picture, string fileName)
        {
            AzureBlob storage = new AzureBlob();

            bool error = false;

            if (ethernetJ11D.IsNetworkUp)
            {
                try
                {
                    storage.PutBlobMobile(fileName, picture.PictureData);
                    //storage.PutBlob("photocontainer",fileName, picture.PictureData, error);
                    if (error)
                    {
                        Debug.Print("Error: " + error.ToString());
                    }
                    else
                    {
                        Debug.Print("There was no error via PutBlob.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print("EXCEPTION: " + ex.Message);
                }
            }
            else
            {
                Debug.Print("NO NETWORK CONNECTION");
            }
        }

        private void saveImageSDCard(GT.Picture picture, string fileName)
        {
            if (verifySDCard())
            {
                string pathFileName = "\\SD\\" + fileName;
                Debug.Print("pathFileName: " + pathFileName);

                try
                {
                    //sdCard.StorageDevice.WriteFile(pathFileName, picture.PictureData);
                    Debug.Print("Image saved to SD Card: " + pathFileName);
                }
                catch (Exception ex)
                {
                    Debug.Print("Error: " + ex.Message);
                }
            }
        }

        private bool verifySDCard()
        {
            if (!sdCard.IsCardInserted || !sdCard.IsCardMounted)
            {
                Debug.Print("Insert SD card!");
                return false;
            }
            return true;
        }

        void readFileFromSDCardSaveToAzure()
        {
            //if (verifySDCard())
            //{
            //    string fileName = "???.bmp";
            //    string pathFileName = "\\SD\\" + fileName;
            //    Debug.Print("pathFileName: " + pathFileName);

            //    try
            //    {
            //        //GT.Picture picture = new GT.Picture(sdCard.StorageDevice.ReadFile(pathFileName), GT.Picture.PictureEncoding.BMP);
            //        Debug.Print("picture.PictureData.Length: " + picture.PictureData.Length.ToString());
            //        if (picture.PictureData.Length > 0)
            //        {
            //            string newFileName = Guid.NewGuid().ToString() + ".bmp";
            //            insertImageintoAzureBlob(picture, newFileName);
            //        }
            //        else
            //        {
            //            Debug.Print("Image not found, has a length <= 0 - " + pathFileName);
            //        }

            //    }
            //    catch (Exception ex)
            //    {
            //        Debug.Print("Error: " + ex.Message);
            //        //multicolorLED.BlinkRepeatedly(GT.Color.Red);
            //    }
            //}
        }

        private void testCamera(){
            try
            {
                if (camera.CameraReady)
                {
                    camera.TakePicture();
                }
                else
                {
                    Debug.Print("Camera not ready, no picutre taken");
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        private void button_ButtonPressed(Button sender, Button.ButtonState state)
        {
            try
            {
                if (camera.CameraReady)
                {
                    camera.TakePicture();
                }
                else
                {
                    Debug.Print("Camera not ready, no picutre taken");
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }

        }
        
    }
}
