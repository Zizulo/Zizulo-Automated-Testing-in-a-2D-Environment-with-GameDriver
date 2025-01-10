using System;
using NUnit.Framework;
using gdio.unity_api;
using gdio.unity_api.v2;
using gdio.common.objects;

namespace Correct2D
{
    [TestFixture]
    public class DemoTest
    {
        //static string path = @"/path/to/macOS/executable/2DDemo.app/Contents/MacOS/2DDemo";
        //static string path = @"C:\path\to\windows\executable\2DDemo\2DDemo.exe";
        static string path = null;

        //static string mode = "standalone";
        static string mode = "IDE";

        //These parameters can be used to override settings used to test when running from the NUnit command line
        public string testMode = TestContext.Parameters.Get("Mode", mode);
        public string pathToExe = TestContext.Parameters.Get("pathToExe", path);

        ApiClient api;

        [OneTimeSetUp]
        public void Connect()
        {
            try
            {
                api = new ApiClient();

                if (pathToExe != null)
                {
                    ApiClient.Launch(pathToExe);
                    api.Connect("localhost", 19734, false, 30);
                }

                else if (testMode == "IDE")
                {
                    api.Connect("localhost", 19734, true, 30);
                }
                else api.Connect("localhost", 19734, false, 30);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            api.EnableHooks(HookingObject.MOUSE);
            api.EnableHooks(HookingObject.KEYBOARD);

            //Start the Game
            api.WaitForObject("//*[@name='StartButton']");
            api.ClickObject(MouseButtons.LEFT, "//*[@name='StartButton']", 30);
            api.Wait(3000);
        }
        [Test, Order(0)]
        public void Zone1()
        {
            api.WaitForObject("//*[@name='Ellen']");

            // Move Ellen to the first InfoPost
            Vector3 infoPost0 = api.GetObjectPosition("/*[@name='InfoPost']");
            api.Wait(1000);
            api.SetObjectFieldValue("//*[@name='Ellen']/fn:component('UnityEngine.Transform')", "position", infoPost0);
            api.Wait(1000);

            // Check that the InfoPost triggers the pop-up dialog
            Assert.That(api.WaitForObjectValue("/*[@name='DialogueCanvas']", "active", true), "InfoPost pop-up failed!");


            // Move Ellen to the second InfoPost ~~ //*[@name='InfoPost']
            Vector3 infoPost1 = api.GetObjectPosition("//*[@name='InfoPost'][1]");
            api.Wait(1000);
            api.SetObjectFieldValue("//*[@name='Ellen']/fn:component('UnityEngine.Transform')", "position", infoPost1);
            api.Wait(1000);

            // Check the the second InfoPost triggers the pop-up dialog
            Assert.That(api.WaitForObjectValue("/*[@name='DialogueCanvas']", "active", true), "InfoPost pop-up failed!");

            // Press down + jump to take us to Zone2
            api.KeyPress(new KeyCode[] { KeyCode.S }, (ulong)api.GetLastFPS());
            api.Wait(500);
            api.KeyPress(new KeyCode[] { KeyCode.Space }, (ulong)api.GetLastFPS() / 2);
            api.Wait(2000);

            // Allow the scene time to load
            api.WaitForObject("/Untagged[@name='KeyCanvas']/Untagged[@name='KeyIcon(Clone)']/Untagged[@name='Key' and ./fn:component('UnityEngine.Behaviour')/@isActiveAndEnabled = 'True']");

            // Check that we teleported to Zone2
            Assert.That("Zone2", Is.EqualTo(api.GetSceneName()), "Wrong zone!");

            api.Wait(1000);

            api.CaptureScreenshot("Zone2-1.jpg", false, true);
        }

        [Test, Order(1)]
        public void Zone2()
        {

            // Move Ellen to the key, and check that it lands in our inventory
            Vector3 key1 = api.GetObjectPosition("//*[@name='Key']");
            // Vector3 key1 = api.GetObjectPosition("//*[@name='Key'][3]");
            api.SetObjectFieldValue("//*[@name='Ellen']/fn:component('UnityEngine.Transform')", "position", key1);
            api.Wait(1000);
            var color = api.GetObjectFieldValue<Color>("/*[@name='KeyCanvas']/*[@name='KeyIcon(Clone)'][0]/*[@name='Key']/fn:component('UnityEngine.UI.Image')/@color");
            Assert.That(color, Is.EqualTo(new Color(1.0f, 1.0f, 1.0f, 1.0f)), "The color does not match the expected value.");

            //Assert.That(api.GetObjectFieldValue<Color>("/*[@name='KeyCanvas']/*[@name='KeyIcon(Clone)'][0]/*[@name='Key']/fn:component('UnityEngine.UI.Image')/@color").ToString(), Is.EqualTo("RGBA(1.000, 1.000, 1.000, 1.000)"), "Its not White");

            // Move Ellen to the entrance for Zone3 and move right
            api.SetObjectFieldValue("//*[@name='Ellen']/fn:component('UnityEngine.Transform')", "position", new Vector3(37, -7, 0));
            api.Wait(1000);
            api.KeyPress(new KeyCode[] { KeyCode.D }, (ulong)api.GetLastFPS());
            api.Wait(500);

            // Allow the scene time to load
            api.WaitForObject("/*[@name='WeaponPickup']");

            // Check that we're in Zone3
            Assert.That("Zone3", Is.EqualTo(api.GetSceneName()), "Wrong zone!");

            api.Wait(1000);

            api.CaptureScreenshot("Zone3.jpg", false, true);
        }

        [Test, Order(2)]
        public void Zone3()
        {
            // Get the weapon and check that we can use it later
            Vector3 weaponVector = api.GetObjectPosition("/*[@name='WeaponPickup']");
            api.SetObjectFieldValue("//*[@name='Ellen']/fn:component('UnityEngine.Transform')", "position", weaponVector);
            // break something just to be sure

            Vector3 key2 = api.GetObjectPosition("/*[@name='Key']");
            api.SetObjectFieldValue("//*[@name='Ellen']/fn:component('UnityEngine.Transform')", "position", key2);
            api.Wait(3000);
            var color = api.GetObjectFieldValue<Color>("/*[@name='KeyCanvas']/*[@name='KeyIcon(Clone)'][1]/*[@name='Key']/fn:component('UnityEngine.UI.Image')/@color");
            Assert.That(color, Is.EqualTo(new Color(1.0f, 1.0f, 1.0f, 1.0f)), "The color does not match the expected value.");

            //Assert.That(api.GetObjectFieldValue<Color>("/*[@name='KeyCanvas']/*[@name='KeyIcon(Clone)'][1]/*[@name='Key']/fn:component('UnityEngine.UI.Image')/@color").ToString(), Is.EqualTo("RGBA(1.000, 1.000, 1.000, 1.000)"), "Its not White");

            // Move left and back to Zone2
            api.KeyPress(new KeyCode[] { KeyCode.A }, (ulong)api.GetLastFPS() * 4);
            api.Wait(500);
            api.KeyPress(new KeyCode[] { KeyCode.Space }, 60);
            api.Wait(4000);

            // Allow the scene time to load
            api.WaitForObject("//*[@name='DestructableColumn'][1]"); // This is the wall we're going to break

            // Check that we're back in Zone2
            Assert.That("Zone2", Is.EqualTo(api.GetSceneName()), "Wrong zone!");

            api.Wait(1000);

            api.CaptureScreenshot("Zone2-2.jpg", false, true);
        }


        [Test, Order(3)]
        public void Zone4()
        {
            // Enables melee attacking using //Player[@name='Ellen']/fn:component('Gamekit2D.PlayerInput') method "EnableMeleeAttacking()"
            //api.CallMethod("//Player[@name='Ellen']/fn:component('Gamekit2D.PlayerInput')", "EnableMeleeAttacking", null);

            // Get the position of the Destructable Column and put Ellen beside it
            Vector3 wall = api.GetObjectPosition("//*[@name='DestructableColumn'][1]");
            api.SetObjectFieldValue("//*[@name='Ellen']/fn:component('UnityEngine.Transform')", "position", new Vector3(wall.x + 1, wall.y, 0));

            // Break the wall!
            api.KeyPress(new KeyCode[] { KeyCode.A }, 30);
            api.Wait(200);
            api.KeyPress(new KeyCode[] { KeyCode.K }, 30);
            api.Wait(200);
            api.KeyPress(new KeyCode[] { KeyCode.A }, (ulong)api.GetLastFPS() * 8);
            api.Wait(10000);


            // Jump to the key, this level is difficult and we can test it's functionality separately
            Vector3 key3 = api.GetObjectPosition("/*[@name='Key']");
            api.SetObjectFieldValue("//*[@name='Ellen']/fn:component('UnityEngine.Transform')", "position", new Vector3(key3.x, key3.y - 1, 0));
            api.Wait(1000);
            var color = api.GetObjectFieldValue<Color>("/*[@name='KeyCanvas']/*[@name='KeyIcon(Clone)'][2]/*[@name='Key']/fn:component('UnityEngine.UI.Image')/@color");
            Assert.That(color, Is.EqualTo(new Color(1.0f, 1.0f, 1.0f, 1.0f)), "The color does not match the expected value.");

            //Assert.That(api.GetObjectFieldValue<Color>("/*[@name='KeyCanvas']/*[@name='KeyIcon(Clone)'][2]/*[@name='Key']/fn:component('UnityEngine.UI.Image')/@color").ToString(), Is.EqualTo("RGBA(1.000, 1.000, 1.000, 1.000)"), "Is not Equal To White");

            // Move to the right for a few seconds - need to enable the 
            api.KeyPress(new KeyCode[] { KeyCode.D }, (ulong)api.GetLastFPS() * 2);

            // Back to Zone 2
            api.SetObjectFieldValue("//*[@name='Ellen']/fn:component('UnityEngine.Transform')", "position", new Vector3(5, 1, 0));
            api.Wait(200);
            api.KeyPress(new KeyCode[] { KeyCode.D }, (ulong)api.GetLastFPS());
            api.Wait(2000);

            // Allow the scene time to load
            api.WaitForObject("//*[@name='PortalInfoPost']");

            // Check that we're back in Zone2
            Assert.That("Zone2", Is.EqualTo(api.GetSceneName()), "Wrong zone!");

            api.Wait(1000);

            api.CaptureScreenshot("Zone2-3.jpg", false, true);
        }

        [Test, Order(4)]
        public void Zone5()
        {

            // Move Ellen to the Portal and Enter!
            Vector3 portal = api.GetObjectPosition("//*[@name='PortalInfoPost']");
            api.SetObjectFieldValue("//*[@name='Ellen']/fn:component('UnityEngine.Transform')", "position", portal);
            api.KeyPress(new KeyCode[] { KeyCode.E }, 30);

            api.WaitForObject("//*[@name='BossDoor']"); // If the boss door loads, we're in

            Assert.That("Zone5", Is.EqualTo(api.GetSceneName()), "Wrong zone, we didn't enter the portal!");

            // Move to the boss
            api.KeyPress(new KeyCode[] { KeyCode.D }, (ulong)api.GetLastFPS() * 10);

            api.CaptureScreenshot("Zone5.jpg", false, true);
            // End Demo

        }

        [OneTimeTearDown]
        public void Disconnect()
        {
            api.DisableHooks(HookingObject.ALL);
            api.DisableObjectCaching();
            api.Disconnect();

            if (testMode == "IDE")
            {
                // Comment this out to leave the editor open after the test, and continue playing manually
                //api.StopEditorPlay();
            }
            else if (testMode == "standalone")
            {
                // Comment this out to leave the player open after the test, and continue playing manually
                //ApiClient.TerminateGame();
            }
        }
    }
}
