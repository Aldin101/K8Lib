using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Reflection;

public class SettingsManager
{
    public static List<ISettingsElement> settingsElements = new List<ISettingsElement>();
        
    public void scrollFix()
    {
        GameObject settingsMenu = GameObject.Find("SettingsMenu");
        if (settingsMenu == null) return;

        GameObject GTTOD = settingsMenu.transform.GetRoot().gameObject;
        GameObject settings = GameObject.Find("SETTINGS");
        GameObject controls = GameObject.Find("CONTROLS");
        GameObject graphics = GameObject.Find("GRAPHICS");
        GameObject application = GameObject.Find("APPLICATION");
        GameObject stats = GameObject.Find("STATS");

        GameObject activeObject = settings ?? controls ?? graphics ?? application ?? stats;

        if (activeObject == null) return;

        var optionsMenu = GTTOD.GetComponent<ac_OptionsMenu>();
        var field = typeof(ac_OptionsMenu).GetField("MaxScrollValue", BindingFlags.NonPublic | BindingFlags.Instance);
        switch (activeObject.name)
        {
            case "SETTINGS":
                field.SetValue(optionsMenu, 150);
                break;
            case "CONTROLS":
                field.SetValue(optionsMenu, 1020);
                break;
            case "GRAPHICS":
                field.SetValue(optionsMenu, 450);
                break;
            case "APPLICATION":
                field.SetValue(optionsMenu, 225 - (50 * (settingsElements.Count + 4)));
                break;
            case "STATS":
                field.SetValue(optionsMenu, 0);
                break;
        }
    }

    public interface ISettingsElement
    {
    }

    public class SettingsElement
    {
        public class TitleBar : ISettingsElement
        {
            public TitleBar(string name, string text)
            {
                GameObject applicationSettingsMenu = GameObject.Find("APPLICATION");
                if (applicationSettingsMenu == null)
                {
                    Debug.LogError("Application settings menu not found");
                    return;
                }

                GameObject inGameTitleBar = applicationSettingsMenu.transform.Find("------MODS------").gameObject;
                if (inGameTitleBar == null)
                {
                    Debug.LogError("In game title bar not found");
                    return;
                }

                GameObject titleBar = GameObject.Instantiate(inGameTitleBar, applicationSettingsMenu.transform, false);
                titleBar.name = name;
                titleBar.transform.localPosition = new Vector3(0, 225 - (50 * (settingsElements.Count + 3)), 0);

                Text titleBarText = titleBar.transform.Find("Text").gameObject.GetComponent<Text>();
                titleBarText.text = text;

                settingsElements.Add(this);
            }
        }

        public class CheckBox : ISettingsElement
        {
            public CheckBox(string name, string text, bool startingState, Action<bool> onValueChanged)
            {
                GameObject applicationSettingsMenu = GameObject.Find("APPLICATION");
                if (applicationSettingsMenu == null)
                {
                    Debug.LogError("Application settings menu not found");
                    return;
                }

                GameObject GTTOD = applicationSettingsMenu.transform.GetRoot().gameObject;
                ac_OptionsMenu optionsMenu = GTTOD.GetComponent<ac_OptionsMenu>();
                GameObject graphicsMenu = optionsMenu.OptionScreens[2].gameObject;
                if (graphicsMenu == null)
                {
                    Debug.LogError("Graphics menu not found");
                    return;
                }

                GameObject inGameCheckBox = graphicsMenu.transform.Find("Fullscreen").gameObject;
                if (inGameCheckBox == null)
                {
                    Debug.LogError("In game check box not found");
                    return;
                }


                GameObject checkBox = GameObject.Instantiate(inGameCheckBox, applicationSettingsMenu.transform, false);
                checkBox.name = name;
                checkBox.transform.localPosition = new Vector3(0, 225 - (50 * (settingsElements.Count + 3)), 0);

                checkBox.transform.Find("FullscreenToggle").gameObject.name = $"{name}Toggle";

                Text checkBoxText = checkBox.transform.Find("OptionTitleLeft").Find("Text").GetComponent<Text>();
                checkBoxText.text = text;

                Toggle toggle = checkBox.transform.Find($"{name}Toggle").gameObject.GetComponent<Toggle>();
                Toggle.DestroyImmediate(toggle);
                toggle = checkBox.transform.Find($"{name}Toggle").gameObject.AddComponent<Toggle>();
                toggle.onValueChanged.AddListener(new UnityAction<bool>(onValueChanged));

                toggle.graphic = checkBox.transform.Find($"{name}Toggle").gameObject.
                    transform.Find("Background").gameObject.
                    transform.Find("Checkmark").gameObject.GetComponent<Image>();

                toggle.isOn = startingState;

                settingsElements.Add(this);
            }
        }

        public class Slider : ISettingsElement
        {
            public Slider(string name, string text, float startingValue, float minValue, float maxValue, bool useWholeNumbers, Action<float> onValueChanged)
            {
                GameObject applicationSettingsMenu = GameObject.Find("APPLICATION");
                if (applicationSettingsMenu == null)
                {
                    Debug.LogError("Application settings menu not found");
                    return;
                }

                GameObject GTTOD = applicationSettingsMenu.transform.GetRoot().gameObject;
                ac_OptionsMenu optionsMenu = GTTOD.GetComponent<ac_OptionsMenu>();
                GameObject graphicsMenu = optionsMenu.OptionScreens[2].gameObject;
                if (graphicsMenu == null)
                {
                    Debug.LogError("Graphics menu not found");
                    return;
                }

                GameObject inGameSlider = graphicsMenu.transform.Find("FieldOfView").gameObject;
                if (inGameSlider == null)
                {
                    Debug.LogError("In game slider not found");
                    return;
                }

                GameObject slider = GameObject.Instantiate(inGameSlider, applicationSettingsMenu.transform, false);
                slider.name = name;
                slider.transform.localPosition = new Vector3(0, 225 - (50 * (settingsElements.Count + 3)), 0);

                Text sliderText = slider.transform.Find("OptionTitleLeft").Find("Text").GetComponent<Text>();
                sliderText.text = text;

                GameObject settingsUI = slider.transform.Find("SettingUI").gameObject;

                settingsUI.transform.Find("FieldOfViewSlider").gameObject.name = $"{name}Slider";
                settingsUI.transform.Find("FieldOfViewInput").gameObject.name = $"{name}Input";
                settingsUI.transform.Find($"{name}Input").transform.
                    Find("FieldOfViewInput Input Caret").gameObject.name = $"{name}InputCaret";

                UnityEngine.UI.Slider sliderBody = settingsUI.transform.Find($"{name}Slider").gameObject.GetComponent<UnityEngine.UI.Slider>();
                UnityEngine.UI.Slider.DestroyImmediate(sliderBody);
                sliderBody = settingsUI.transform.Find($"{name}Slider").gameObject.AddComponent<UnityEngine.UI.Slider>();

                sliderBody.onValueChanged.RemoveAllListeners();
                sliderBody.onValueChanged.AddListener(new UnityAction<float>(onValueChanged));
                sliderBody.minValue = minValue;
                sliderBody.maxValue = maxValue;
                sliderBody.wholeNumbers = useWholeNumbers;

                sliderBody.image = settingsUI.transform.Find($"{name}Slider").transform.
                    Find("Handle Slide Area").gameObject.transform.
                    Find("Handle").gameObject.GetComponent<Image>();

                sliderBody.handleRect = settingsUI.transform.Find($"{name}Slider").transform.
                    Find("Handle Slide Area").gameObject.transform.
                    Find("Handle").gameObject.GetComponent<RectTransform>();

                sliderBody.fillRect = settingsUI.transform.Find($"{name}Slider").transform.
                    Find("Fill Area").transform.
                    Find("Fill").gameObject.GetComponent<RectTransform>();


                InputField input = settingsUI.transform.Find($"{name}Input").gameObject.GetComponent<InputField>();
                InputField.DestroyImmediate(input);
                input = settingsUI.transform.Find($"{name}Input").gameObject.AddComponent<InputField>();
                input.onSubmit.RemoveAllListeners();
                input.onSubmit.AddListener((string value) =>
                {
                    float newValue;
                    if (float.TryParse(value, out newValue))
                    {
                        if (newValue < minValue)
                        {
                            newValue = minValue;
                        }
                        else if (newValue > maxValue)
                        {
                            newValue = maxValue;
                        }
                        sliderBody.value = newValue;
                    }
                });

                sliderBody.onValueChanged.AddListener((float value) =>
                {
                    input.transform.Find("Text").GetComponent<Text>().text = value.ToString();
                });

                sliderBody.value = startingValue;

                settingsElements.Add(this);
            }
        }

        public class DropDown : ISettingsElement
        {
            public DropDown(string name, string text, List<string> elements, int defaultindex, Action<int> OnValueChanged)
            {
                List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
                foreach (string element in elements)
                {
                    options.Add(new Dropdown.OptionData(element));
                }

                GameObject applicationSettingsMenu = GameObject.Find("APPLICATION");
                if (applicationSettingsMenu == null)
                {
                    Debug.LogError("Application settings menu not found");
                    return;
                }

                GameObject GTTOD = applicationSettingsMenu.transform.GetRoot().gameObject;
                ac_OptionsMenu optionsMenu = GTTOD.GetComponent<ac_OptionsMenu>();
                GameObject graphicsMenu = optionsMenu.OptionScreens[2].gameObject;
                if (graphicsMenu == null)
                {
                    Debug.LogError("Graphics menu not found");
                    return;
                }

                GameObject resolutionPicker = graphicsMenu.transform.Find("Resolution").gameObject;
                if (resolutionPicker == null)
                {
                    Debug.LogError("Resolution picker not found");
                    return;
                }

                GameObject referenceChild = resolutionPicker.transform.Find("ResolutionDropdown").gameObject;
                GameObject dropdown = GameObject.Instantiate(resolutionPicker, applicationSettingsMenu.transform, false);
                dropdown.name = name;
                dropdown.transform.localPosition = new Vector3(0, 225 - (50 * (settingsElements.Count + 3)), 0);
                dropdown.transform.Find("OptionTitleLeft").Find("Text").gameObject.GetComponent<Text>().text = text;
                dropdown.transform.Find("ResolutionDropdown").gameObject.name = $"{name}Dropdown";

                Dropdown dropdownComponent = dropdown.transform.Find($"{name}Dropdown").gameObject.GetComponent<Dropdown>();
                var originalValues = new Dictionary<string, object>();
                foreach (var prop in dropdownComponent.GetType().GetProperties())
                {
                    if (prop.CanRead)
                    {
                        originalValues[prop.Name] = prop.GetValue(dropdownComponent, null);
                    }
                }

                Dropdown.DestroyImmediate(dropdownComponent);
                dropdownComponent = dropdown.transform.Find($"{name}Dropdown").gameObject.AddComponent<Dropdown>();

                foreach (var prop in dropdownComponent.GetType().GetProperties())
                {
                    if (prop.Name != "onValueChanged" && prop.CanWrite && originalValues.ContainsKey(prop.Name))
                    {
                        prop.SetValue(dropdownComponent, originalValues[prop.Name], null);
                    }
                }

                dropdownComponent.options = options;
                dropdownComponent.value = defaultindex;

                dropdownComponent.onValueChanged.RemoveAllListeners();
                dropdownComponent.onValueChanged.AddListener(new UnityAction<int>(OnValueChanged));

                settingsElements.Add(this);
            }
        }

        public class TextInput : ISettingsElement
        {

        }
    }
}
