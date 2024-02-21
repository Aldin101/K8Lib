using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Reflection;
using static SettingsManager.SettingsElement;

public class SettingsManager
{
    private static List<ISettingsElement> settingsElements = new List<ISettingsElement>();
    private static GameManager GM = GameManager.GM;

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
                GameObject GTTOD = GM.gameObject;
                if (GTTOD == null)
                {
                    Debug.LogError("GTTOD not found");
                    return;
                }

                GameObject applicationSettingsMenu = GTTOD.GetComponent<ac_OptionsMenu>().OptionScreens[3].gameObject;

                if (applicationSettingsMenu == null)
                {
                    Debug.LogError("Application settings menu not found");
                    return;
                }

                if (applicationSettingsMenu.activeSelf == false)
                {
                    return;
                }

                GameObject inGameTitleBar = applicationSettingsMenu.transform.GetChild(1).gameObject;

                if (inGameTitleBar == null)
                {
                    Debug.LogError("In game title bar not found");
                    return;
                }

                Debug.Log("2");

                GameObject titleBar = GameObject.Instantiate(inGameTitleBar, applicationSettingsMenu.transform, false);
                titleBar.name = name;
                titleBar.transform.localPosition = new Vector3(0, 225 - (50 * (settingsElements.Count + 3)), 0);

                Text titleBarText = titleBar.GetComponentsInChildren<Text>()[0];

                titleBarText.text = text;

                settingsElements.Add(this);
            }
        }

        public class CheckBox : ISettingsElement
        {
            public CheckBox(string name, string text, bool startingState, Action<bool> onValueChanged)
            {
                GameObject GTTOD = GM.gameObject;
                if (GTTOD == null)
                {
                    Debug.LogError("GTTOD not found");
                    return;
                }

                GameObject applicationSettingsMenu = GTTOD.GetComponent<ac_OptionsMenu>().OptionScreens[3].gameObject;

                if (applicationSettingsMenu == null)
                {
                    Debug.LogError("Application settings menu not found");
                    return;
                }

                if (applicationSettingsMenu.activeSelf == false)
                {
                    return;
                }

                ac_OptionsMenu optionsMenu = GTTOD.GetComponent<ac_OptionsMenu>();
                GameObject graphicsMenu = GTTOD.GetComponent<ac_OptionsMenu>().OptionScreens[2].gameObject;
                if (graphicsMenu == null)
                {
                    Debug.LogError("Graphics menu not found");
                    return;
                }

                GameObject inGameCheckBox = graphicsMenu.transform.GetChild(3).gameObject;
                if (inGameCheckBox == null)
                {
                    Debug.LogError("In game check box not found");
                    return;
                }


                GameObject checkBox = GameObject.Instantiate(inGameCheckBox, applicationSettingsMenu.transform, false);
                checkBox.name = name;
                checkBox.transform.localPosition = new Vector3(0, 225 - (50 * (settingsElements.Count + 3)), 0);

                checkBox.transform.GetChild(1).gameObject.name = $"{name}Toggle";

                Text checkBoxText = checkBox.transform.GetChild(0).GetChild(0).GetComponent<Text>();
                checkBoxText.text = text;

                Toggle toggle = checkBox.transform.GetChild(1).gameObject.GetComponent<Toggle>();
                Toggle.DestroyImmediate(toggle);
                toggle = checkBox.transform.GetChild(1).gameObject.AddComponent<Toggle>();
                toggle.onValueChanged.AddListener(new UnityAction<bool>(onValueChanged));

                toggle.graphic = checkBox.transform.GetChild(1).gameObject.
                    transform.GetChild(0).gameObject.
                    transform.GetChild(0).gameObject.GetComponent<Image>();

                toggle.isOn = startingState;

                settingsElements.Add(this);
            }
        }

        public class Slider : ISettingsElement
        {
            public Slider(string name, string text, float startingValue, float minValue, float maxValue, bool useWholeNumbers, Action<float> onValueChanged)
            {
                GameObject GTTOD = GM.gameObject;
                if (GTTOD == null)
                {
                    Debug.LogError("GTTOD not found");
                    return;
                }

                GameObject applicationSettingsMenu = GTTOD.GetComponent<ac_OptionsMenu>().OptionScreens[3].gameObject;

                if (applicationSettingsMenu == null)
                {
                    Debug.LogError("Application settings menu not found");
                    return;
                }

                if (applicationSettingsMenu.activeSelf == false)
                {
                    return;
                }

                ac_OptionsMenu optionsMenu = GTTOD.GetComponent<ac_OptionsMenu>();
                GameObject graphicsMenu = GTTOD.GetComponent<ac_OptionsMenu>().OptionScreens[2].gameObject;
                if (graphicsMenu == null)
                {
                    Debug.LogError("Graphics menu not found");
                    return;
                }

                GameObject inGameSlider = graphicsMenu.transform.GetChild(5).gameObject;
                if (inGameSlider == null)
                {
                    Debug.LogError("In game slider not found");
                    return;
                }

                GameObject slider = GameObject.Instantiate(inGameSlider, applicationSettingsMenu.transform, false);
                slider.name = name;
                slider.transform.localPosition = new Vector3(0, 225 - (50 * (settingsElements.Count + 3)), 0);

                Text sliderText = slider.transform.GetChild(0).GetChild(0).GetComponent<Text>();
                sliderText.text = text;

                GameObject settingsUI = slider.transform.GetChild(1).gameObject;

                settingsUI.transform.GetChild(0).gameObject.name = $"{name}Slider";
                settingsUI.transform.GetChild(1).gameObject.name = $"{name}Input";
                settingsUI.transform.GetChild(1).transform.
                    GetChild(0).gameObject.name = $"{name} Input Caret";

                UnityEngine.UI.Slider sliderBody = settingsUI.transform.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Slider>();
                UnityEngine.UI.Slider.DestroyImmediate(sliderBody);
                sliderBody = settingsUI.transform.GetChild(0).gameObject.AddComponent<UnityEngine.UI.Slider>();

                sliderBody.onValueChanged.RemoveAllListeners();
                sliderBody.onValueChanged.AddListener(new UnityAction<float>(onValueChanged));
                sliderBody.minValue = minValue;
                sliderBody.maxValue = maxValue;
                sliderBody.wholeNumbers = useWholeNumbers;

                sliderBody.image = settingsUI.transform.GetChild(0).transform.
                    GetChild(2).gameObject.transform.
                    GetChild(0).gameObject.GetComponent<Image>();

                sliderBody.handleRect = settingsUI.transform.GetChild(0).transform.
                    GetChild(2).gameObject.transform.
                    GetChild(0).gameObject.GetComponent<RectTransform>();

                sliderBody.fillRect = settingsUI.transform.GetChild(0).transform.
                    GetChild(1).transform.
                    GetChild(0).gameObject.GetComponent<RectTransform>();


                InputField input = settingsUI.transform.GetChild(1).gameObject.GetComponent<InputField>();
                InputField.DestroyImmediate(input);
                input = settingsUI.transform.GetChild(1).gameObject.AddComponent<InputField>();
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
                    input.transform.GetChild(2).GetComponent<Text>().text = value.ToString();
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

                GameObject GTTOD = GM.gameObject;
                if (GTTOD == null)
                {
                    Debug.LogError("GTTOD not found");
                    return;
                }

                GameObject applicationSettingsMenu = GTTOD.GetComponent<ac_OptionsMenu>().OptionScreens[3].gameObject;

                if (applicationSettingsMenu == null)
                {
                    Debug.LogError("Application settings menu not found");
                    return;
                }

                if (applicationSettingsMenu.activeSelf == false)
                {
                    return;
                }

                ac_OptionsMenu optionsMenu = GTTOD.GetComponent<ac_OptionsMenu>();
                GameObject graphicsMenu = GTTOD.GetComponent<ac_OptionsMenu>().OptionScreens[2].gameObject;
                if (graphicsMenu == null)
                {
                    Debug.LogError("Graphics menu not found");
                    return;
                }

                GameObject resolutionPicker = graphicsMenu.transform.GetChild(2).gameObject;
                if (resolutionPicker == null)
                {
                    Debug.LogError("Resolution picker not found");
                    return;
                }

                GameObject referenceChild = resolutionPicker.transform.GetChild(1).gameObject;
                GameObject dropdown = GameObject.Instantiate(resolutionPicker, applicationSettingsMenu.transform, false);
                dropdown.name = name;
                dropdown.transform.localPosition = new Vector3(0, 225 - (50 * (settingsElements.Count + 3)), 0);
                dropdown.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Text>().text = text;
                dropdown.transform.GetChild(1).gameObject.name = $"{name}Dropdown";

                Dropdown dropdownComponent = dropdown.transform.GetChild(1).gameObject.GetComponent<Dropdown>();
                var originalValues = new Dictionary<string, object>();
                foreach (var prop in dropdownComponent.GetType().GetProperties())
                {
                    if (prop.CanRead)
                    {
                        originalValues[prop.Name] = prop.GetValue(dropdownComponent, null);
                    }
                }

                Dropdown.DestroyImmediate(dropdownComponent);
                dropdownComponent = dropdown.transform.GetChild(1).gameObject.AddComponent<Dropdown>();

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
            public TextInput(string name, string text, string existingText, string placeholderText, Action<string> OnValueChange)
            {
                GameObject GTTOD = GM.gameObject;
                if (GTTOD == null)
                {
                    Debug.LogError("GTTOD not found");
                    return;
                }

                GameObject applicationSettingsMenu = GTTOD.GetComponent<ac_OptionsMenu>().OptionScreens[3].gameObject;

                if (applicationSettingsMenu == null)
                {
                    Debug.LogError("Application settings menu not found");
                    return;
                }

                if (applicationSettingsMenu.activeSelf == false)
                {
                    return;
                }

                GameObject inGameTextInput = applicationSettingsMenu.transform.GetChild(2).gameObject;
                if (inGameTextInput == null)
                {
                    Debug.LogError("In game text input not found");
                    return;
                }

                GameObject textInput = GameObject.Instantiate(inGameTextInput, applicationSettingsMenu.transform, false);
                textInput.name = name;
                textInput.transform.localPosition = new Vector3(0, 225 - (50 * (settingsElements.Count + 3)), 0);

                textInput.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Text>().text = text;
                textInput.transform.GetChild(1).gameObject.name = $"{name}Input";

                InputField inputComponent = textInput.transform.GetChild(1).gameObject.GetComponent<InputField>();
                var originalValues = new Dictionary<string, object>();
                foreach (var prop in inputComponent.GetType().GetProperties())
                {
                    if (prop.CanRead)
                    {
                        originalValues[prop.Name] = prop.GetValue(inputComponent, null);
                    }
                }

                InputField.DestroyImmediate(inputComponent);
                inputComponent = textInput.transform.GetChild(1).gameObject.AddComponent<InputField>();

                foreach (var prop in inputComponent.GetType().GetProperties())
                {
                    if (prop.Name != "onValueChanged" && prop.Name != "onSubmit" && prop.CanWrite && originalValues.ContainsKey(prop.Name))
                    {
                        prop.SetValue(inputComponent, originalValues[prop.Name], null);
                    }
                }

                inputComponent.text = existingText;
                inputComponent.placeholder.GetComponent<Text>().text = placeholderText;
                inputComponent.onSubmit.RemoveAllListeners();
                inputComponent.onValueChanged.RemoveAllListeners();

                inputComponent.onValueChanged.AddListener(new UnityAction<string>(OnValueChange));
                inputComponent.onSubmit.AddListener(new UnityAction<string>(OnValueChange));
            }
        }
    }
}
