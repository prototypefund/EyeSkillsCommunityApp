/*
    This code belongs to the "EyeSkills Framework". It is designed to assist
    in the development of software for the human/animal visual system.

    Copyright(C) 2018 Dr. Thomas Benjamin Senior, Michael Zöller.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PowerUI;
using System;
using SubjectNerd.Utilities;
using EyeSkills;

public class EyeSkillsMenu : MonoBehaviour
{
    [System.Serializable]
    public class MenuItem
    {
        public string title, text, scene, description;
        public string logo;
        public string audioId;
    }

    [Reorderable] public MenuItem[] menuItems;

    [Reorderable] public MenuItem[] debugItems;

    public bool skipTitles = false;
    public float powerUILengthScale = 1.8f;
    //public float powerUILengthScale = 3f;

    private int clicks = 0;

    int menuPosition = -1;

    Dom.Element landscapeElement, menuElement;
    bool debugActive = false;

    private int unlocked = 0;

    void BuildMenu(MenuItem[] items)
    {

        AudioManager.instance.Say("Menu");
        string menu = "";
        int i =0;

        //If we are in practitioner mode, unlock all the menu items - choose any scene at will, immediately.
        if (PlayerPrefs.GetString("EyeSkills.practitionerMode") == "1"){
            unlocked = items.Length;
        }

        foreach (MenuItem item in items)
        {
            string l = "";
            if (item.text != "")
                //menu += "<div class='menuitem" + l + "'><div class='menuitem-style'></div><div class='menuitem-logo' style='background-image: url(" + item.logo + ")'></div><div class='menuitem-title' id='menu-" + i + "'>" + item.text + "</div></div>";
                menu += "<div class='menuitem" + l + "'><div class='menuitem-style'></div><div class='menuitem-title' id='menu-" + i + "'>" + item.text + "</div></div>";
            i++;
            NetworkManager.instance.RegisterButton(item.scene, item.text, "");
        }

        UI.Variables["menu"] = menu;

        i = 0;
        foreach (var element in UI.document.body.getElementsByClassName("menuitem-title"))
        {

            element.onmousedown = OnHeaderMouseDown;
        }

        ChangeMenuItem(1, true);
    }

    void Awake()
    {
#if UNITY_EDITOR
        GetComponent<PowerUI.Manager>().LengthScale = powerUILengthScale;
#endif
    }

void EnterScene() {
    GameObject.Find("#PowerUI").SetActive(false);
    if( selectedScene >= unlocked ) {
        unlocked++;
        if( unlocked > 4 ) {
          unlocked = 0;
          PlayerPrefs.SetInt("EyeSkills.Runs", PlayerPrefs.GetInt("EyeSkills.Runs") + 1);
        }
        PlayerPrefs.SetInt("EyeSkills.MenuUnlocked", unlocked);
    }

    if (menuItems[selectedScene].scene == "VirtuallyReal"){
        Debug.Log("Asking for camera permission");
        OnGrantButtonPress();
        Debug.Log("Asking for USB Permission");
        RequestUSBPermissions();          
    }

    UnityEngine.SceneManagement.SceneManager.LoadScene(menuItems[selectedScene].scene);
}


    private bool CheckPermissions()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            return true;
        }
        return AndroidPermissionsManager.IsPermissionGranted("android.permission.CAMERA");
        
    }

    public void RequestUSBPermissions()
    {
        AndroidPermissionsManager.RequestPermission(new[] { "android.permission.USB_PERMISSION" }, new AndroidPermissionCallback(
            grantedPermission =>
            {
                Debug.Log("GRANTED PERMISSION");
                // The permission was successfully granted, restart the change avatar routine
                UnityEngine.SceneManagement.SceneManager.LoadScene(menuItems[selectedScene].scene);
            },
            deniedPermission =>
            {
                Debug.Log("DENIED PERMISSION");
                // The permission was denied
            },
            deniedPermissionAndDontAskAgain =>
            {
                Debug.Log("DENIED PERMISSION AND DONT ASK AGAIN");
                // The permission was denied, and the user has selected "Don't ask again"
                // Show in-game pop-up message stating that the user can change permissions in Android Application Settings
                // if he changes his mind (also required by Google Featuring program)
            }));
    }

    public void OnGrantButtonPress()
    {
        AndroidPermissionsManager.RequestPermission(new []{"android.permission.CAMERA"}, new AndroidPermissionCallback(
            grantedPermission =>
            {
              Debug.Log("GRANTED PERMISSION");
                // The permission was successfully granted, restart the change avatar routine
                UnityEngine.SceneManagement.SceneManager.LoadScene(menuItems[selectedScene].scene);
            },
            deniedPermission =>
            {
              Debug.Log("DENIED PERMISSION");
                // The permission was denied
            },
            deniedPermissionAndDontAskAgain =>
            {
              Debug.Log("DENIED PERMISSION AND DONT ASK AGAIN");
                // The permission was denied, and the user has selected "Don't ask again"
                // Show in-game pop-up message stating that the user can change permissions in Android Application Settings
                // if he changes his mind (also required by Google Featuring program)
            }));
    }

    void EnterSceneLockedEye(bool left)
    {
      EyeSkillsCameraRig.CameraRigConfig config = new EyeSkillsCameraRig.CameraRigConfig();
      config.Load();
      config.leftEyeIsStrabismic = left;
      config.rightEyeIsStrabismic = !left;
      config.Save();
      EnterScene();
    }

    void Start()
    {
        //A hack to disable the gesture based control in the scenes. That is inappropriate while exploring the environments.
        PlayerPrefs.SetString("EyeSkills.practitionerMode", "1"); 

        if (PlayerPrefs.GetString("EyeSkills.Practitioner") == "") {
          //SceneManager.LoadScene("Username");
          PlayerPrefs.SetString("EyeSkills.Name", System.Guid.NewGuid().ToString());
          PlayerPrefs.SetString("EyeSkills.Practitioner", "practitioner1");
        }

        unlocked = PlayerPrefs.GetInt("EyeSkills.MenuUnlocked", 0);

        NetworkManager.instance.RegisterScene("Main Menu", "Select a calibration.");

        landscapeElement = UI.document.getElementById("landscape");
        menuElement = UI.document.getElementById("menu");

        landscapeElement.style.display = "none";
        menuElement.style.display = "none";

        BuildMenu(menuItems);

        UI.document.getById("submenu-button1").addEventListener("mousedown", delegate(MouseEvent e) {
          EnterSceneLockedEye(true);
        });

        UI.document.getById("submenu-button2").addEventListener("mousedown", delegate(MouseEvent e) {
          EnterSceneLockedEye(false);
        });

        UI.document.getById("submenu-button").addEventListener("mousedown", delegate(MouseEvent e) {
          EnterScene();
        });

        UI.document.getById("header").addEventListener("mousedown", delegate(MouseEvent e)
        {
            //clicks++;
            //if (clicks >= 3) {
            //  debugActive = true;
            //  BuildMenu(debugItems);
            //}
            //else {
              debugActive = false;
              BuildMenu(menuItems);
              UI.document.getById("swiper").animate("left:0%", .1f);
            //}
        });

        //StartCoroutine(_checkInternetConnection());
        StartCoroutine(_clickReset());

/*        Debug.Log("Checking Permissions");
        Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.Log("webcam auth ok");
        }
        else
        {
            Debug.Log("webcam auth not ok");
        }*/
    }

    IEnumerator _clickReset() {
      while( true )  {
        yield return new WaitForSeconds(1.5f);
        clicks = 0;
      }
}

    IEnumerator _checkInternetConnection()
    {
        while (true)
        {
            WWW www = new WWW("https://server.eyeskills.org");
            yield return www;
            if (www.error != null)
                UI.Variables["online"] = "no";
            else
                UI.Variables["online"] = "yes";

            yield return new WaitForSeconds(1f);
        }
    }

    void ChangeMenuItem(int pos, bool noSound = false)
    {
        do
        {
            menuPosition += pos;
            if (menuPosition < 0)
                menuPosition = menuItems.Length - 1;
            if (menuPosition >= menuItems.Length)
                menuPosition = 0;
        } while (!skipTitles && menuItems[menuPosition].text == "");

        MenuItem menuItem = menuItems[menuPosition];
        if (menuItem.audioId != "" && !noSound)
            AudioManager.instance.Say(menuItem.audioId);

        Debug.Log("Menu: " + menuItem.title + " " + menuItem.text);
    }

    void Update()
    {
        if (UnityEngine.Input.GetButtonDown("EyeSkills Up"))
            ChangeMenuItem(-1);

        if (UnityEngine.Input.GetButtonDown("EyeSkills Down"))
            ChangeMenuItem(+1);

        if (UnityEngine.Input.GetButtonDown("EyeSkills Confirm") && menuItems[menuPosition].scene != "")
        {
            Debug.Log("EyeSkillsMenu : Loading scene via Physical Button " + menuItems[menuPosition].scene);
            SceneManager.LoadScene(menuItems[menuPosition].scene);
        }

        bool landscape = (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight);
        landscapeElement.style.display = landscape ? "block" : "none";
        menuElement.style.display = landscape ? "none" : "block";

        foreach (MenuItem item in menuItems)
            if (NetworkManager.instance.GetButton(item.scene))
            {
                Debug.Log("EyeSkillsMenu : Loading scene via Web Button " + item.scene);
                //Application.LoadLevel(item.scene);
                SceneManager.LoadScene(item.scene);
            }

        if ( UnityEngine.Input.GetKeyDown(KeyCode.Escape) || UnityEngine.Input.GetButton("EyeSkills Cancel") ) {
          debugActive = false;
          BuildMenu(menuItems);
          UI.document.getById("swiper").animate("left:0%", .1f);
        }

        /*
        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            if (UnityEngine.Input.GetKey(kcode))
                UI.Variables["logger"] = "KeyCode down: " + kcode;
        for(int i=0; i<12; i++)
            if( UnityEngine.Input.GetMouseButton(i) )
                UI.Variables["logger"] = "Mouse button down: " + i;
        for (int i = 0; i < 20; i++)
            if (UnityEngine.Input.GetKeyDown("joystick 1 button " + i))
                UI.Variables["logger"] = "Joystick button down: " + i;
        */
    }

    int selectedScene;

	  public void OnHeaderMouseDown(MouseEvent mouseEvent){
      int index = Int32.Parse(mouseEvent.htmlTarget.getAttribute("id").Replace("menu-", ""));

      if( debugActive ) {
        UnityEngine.SceneManagement.SceneManager.LoadScene(debugItems[index].scene);
        return;
      }

      UI.document.getById("swiper").animate("left:-100%", .15f);
      UI.document.getById("submenu-title").innerHTML = menuItems[index].text;
      UI.document.getById("submenu-text").innerHTML = menuItems[index].description;

      bool isMisalignmentMeasurement = menuItems[index].scene == "MisalignmentMeasurement";

      UI.document.getById("submenu-button").style.display = isMisalignmentMeasurement ? "none" : "block";
      UI.document.getById("submenu-button1").style.display = isMisalignmentMeasurement ? "block" : "none";
      UI.document.getById("submenu-button2").style.display = isMisalignmentMeasurement ? "block" : "none";

      selectedScene = index;
  }

  public static void Donate(MouseEvent mouseEvent) {
    Application.OpenURL("https://www.eyeskills.org/donate/");
  }

}
