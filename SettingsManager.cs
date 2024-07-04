using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Reflection;
using System.Linq;
using System.Collections;
using static UnityEngine.UIElements.StylePropertyAnimationSystem;

namespace K8Lib.Settings
{
    internal static class Manager
    {
        public static Dictionary<string, object> settingsElements = new Dictionary<string, object>();
        public static void scrollFix()
        {
            GameObject application = GameObject.Find("APPLICATION");

            var optionsMenu = GameManager.GM.GetComponent<ac_OptionsMenu>();
            var field = typeof(ac_OptionsMenu).GetField("MaxScrollValue", BindingFlags.NonPublic | BindingFlags.Instance);

            if (application != null)
            {
                int scrollValue = -600 + (50 * (settingsElements.Count + 4));
                if (scrollValue < 0) scrollValue = 0;
                field.SetValue(optionsMenu, scrollValue);
            }
        }

        public static void checkElements()
        {
            GameObject applicationSettingsMenu = GameManager.GM.GetComponent<ac_OptionsMenu>().OptionScreens[3].gameObject;

            if (applicationSettingsMenu == null)
            {
                Debug.LogError("Application settings menu not found");
                return;
            }

            if (applicationSettingsMenu.activeSelf == false)
            {
                return;
            }

            if (applicationSettingsMenu.transform.childCount < 4 + settingsElements.Count)
            {
                restoreElements();
            }
        }

        public static void restoreElements()
        {
            Dictionary<string, object> tempElements = settingsElements;
            settingsElements = new Dictionary<string, object>();

            foreach (var element in tempElements)
            {
                if (element.Value == null) continue;
                if (element.Value is TitleBar)
                {
                    TitleBar titleBar = element.Value as TitleBar;
                    BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<K8Lib>().StartCoroutine(titleBar.createElement(titleBar.name, titleBar.text));
                }
                else if (element.Value is CheckBox)
                {
                    CheckBox checkBox = element.Value as CheckBox;
                    BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<K8Lib>().StartCoroutine(checkBox.createElement(checkBox.name, checkBox.text, checkBox.startingState, checkBox.onValueChanged));
                }
                else if (element.Value is Slider)
                {
                    Slider slider = element.Value as Slider;
                    BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<K8Lib>().StartCoroutine(slider.createElement(slider.name, slider.text, slider.minValue, slider.maxValue, slider.startingValue, slider.useWholeNumbers, slider.onValueChanged));
                }
                else if (element.Value is DropDown)
                {
                    DropDown dropDown = element.Value as DropDown;
                    BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<K8Lib>().StartCoroutine(dropDown.createElement(dropDown.name, dropDown.text, dropDown.elements, dropDown.defaultIndex, dropDown.OnValueChanged));
                }
                else if (element.Value is TextInput)
                {
                    TextInput textInput = element.Value as TextInput;
                    BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<K8Lib>().StartCoroutine(textInput.createElement(textInput.name, textInput.text, textInput.existingText, textInput.placeholderText, textInput.OnSubmit, textInput.OnValueChange));
                }
            }
        }
    }

    public class TitleBar
    {
        public GameObject gameObject;

        internal string name;
        internal string text;

        public TitleBar(string name, string text)
        {
            if (Manager.settingsElements.Keys.Contains(name)) return;

            this.name = name;
            this.text = text;

            BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<K8Lib>().StartCoroutine(createElement(name, text));
        }

        internal IEnumerator createElement(string name, string text)
        {
            start:

            while (!GameManager.GM) yield return null;

            GameObject applicationSettingsMenu = GameManager.GM.GetComponent<ac_OptionsMenu>().OptionScreens[3].gameObject;

            while (!applicationSettingsMenu.activeSelf)
            {
                yield return null;
                if (applicationSettingsMenu == null) goto start;
            }

                GameObject inGameTitleBar = applicationSettingsMenu.transform.GetChild(1).gameObject;

            GameObject titleBar = GameObject.Instantiate(inGameTitleBar, applicationSettingsMenu.transform, false);
            titleBar.name = name;
            titleBar.transform.localPosition = new Vector3(0, 225 - (50 * (Manager.settingsElements.Count + 3)), 0);

            Text titleBarText = titleBar.GetComponentsInChildren<Text>()[0];

            titleBarText.text = text;

            gameObject = titleBar;

            Manager.settingsElements.Add(name, this);
        }
    }

    public class CheckBox
    {
        public GameObject gameObject;

        internal string name;
        internal string text;
        internal bool startingState;
        internal Action<bool> onValueChanged;

        public CheckBox(string name, string text, bool startingState, Action<bool> onValueChanged)
        {
            if (Manager.settingsElements.Keys.Contains(name)) return;

            this.name = name;
            this.text = text;
            this.startingState = startingState;
            this.onValueChanged = onValueChanged;

            BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<K8Lib>().StartCoroutine(createElement(name, text, startingState, onValueChanged));
        }

        internal IEnumerator createElement(string name, string text, bool startingState, Action<bool> onValueChanged)
        {
            start:

            while (!GameManager.GM) yield return null;

            GameObject applicationSettingsMenu = GameManager.GM.GetComponent<ac_OptionsMenu>().OptionScreens[3].gameObject;

            while (!applicationSettingsMenu.activeSelf)
            {
                yield return null;
                if (applicationSettingsMenu == null) goto start;
            }

            ac_OptionsMenu optionsMenu = GameManager.GM.GetComponent<ac_OptionsMenu>();
            GameObject graphicsMenu = GameManager.GM.GetComponent<ac_OptionsMenu>().OptionScreens[2].gameObject;

            GameObject inGameCheckBox = graphicsMenu.transform.GetChild(3).gameObject;


            GameObject checkBox = GameObject.Instantiate(inGameCheckBox, applicationSettingsMenu.transform, false);
            checkBox.name = name;
            checkBox.transform.localPosition = new Vector3(0, 225 - (50 * (Manager.settingsElements.Count + 3)), 0);

            checkBox.transform.GetChild(1).gameObject.name = $"{name}Toggle";

            Text checkBoxText = checkBox.transform.GetChild(0).GetChild(0).GetComponent<Text>();
            checkBoxText.text = text;

            Toggle toggle = checkBox.transform.GetChild(1).gameObject.GetComponent<Toggle>();
            Toggle.DestroyImmediate(toggle);
            toggle = checkBox.transform.GetChild(1).gameObject.AddComponent<Toggle>();
            toggle.onValueChanged.AddListener(new UnityAction<bool>(onValueChanged));
            toggle.onValueChanged.AddListener((bool value) =>
            {
                this.startingState = value;
            });

            toggle.graphic = checkBox.transform.GetChild(1).gameObject.
                transform.GetChild(0).gameObject.
                transform.GetChild(0).gameObject.GetComponent<Image>();

            toggle.isOn = startingState;

            gameObject = checkBox;

            Manager.settingsElements.Add(name, this);
        }
    }

    public class Slider
    {
        public GameObject gameObject;

        internal string name;
        internal string text;
        internal float minValue;
        internal float maxValue;
        internal float startingValue;
        internal bool useWholeNumbers;
        internal Action<float> onValueChanged;

        public Slider(string name, string text, float minValue, float maxValue, float startingValue, bool useWholeNumbers, Action<float> onValueChanged)
        {
            if (Manager.settingsElements.Keys.Contains(name)) return;

            this.name = name;
            this.text = text;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.startingValue = startingValue;
            this.useWholeNumbers = useWholeNumbers;
            this.onValueChanged = onValueChanged;

            BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<K8Lib>().StartCoroutine(createElement(name, text, minValue, maxValue, startingValue, useWholeNumbers, onValueChanged));
        }

        internal IEnumerator createElement(string name, string text, float minValue, float maxValue, float startingValue, bool useWholeNumbers, Action<float> onValueChanged)
        {
            start:

            while (!GameManager.GM) yield return null;

            GameObject applicationSettingsMenu = GameManager.GM.GetComponent<ac_OptionsMenu>().OptionScreens[3].gameObject;

            while (!applicationSettingsMenu.activeSelf)
            {
                yield return null;
                if (applicationSettingsMenu == null) goto start;
            }

            ac_OptionsMenu optionsMenu = GameManager.GM.GetComponent<ac_OptionsMenu>();
            GameObject graphicsMenu = GameManager.GM.GetComponent<ac_OptionsMenu>().OptionScreens[2].gameObject;

            GameObject inGameSlider = graphicsMenu.transform.GetChild(5).gameObject;

            GameObject slider = GameObject.Instantiate(inGameSlider, applicationSettingsMenu.transform, false);

            slider.name = name;
            slider.transform.localPosition = new Vector3(0, 225 - (50 * (Manager.settingsElements.Count + 3)), 0);

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
                //round to 3 decimal places
                float roundedNumber = (float)Math.Round(value, 3);
                input.transform.GetChild(2).GetComponent<Text>().text = roundedNumber.ToString();
                this.startingValue = value;
            });

            sliderBody.value = startingValue;

            gameObject = slider;

            Manager.settingsElements.Add(name, this);
        }
    }

    public class DropDown
    {
        public GameObject gameObject;

        internal string name;
        internal string text;
        internal List<string> elements;
        internal int defaultIndex;
        internal Action<int> OnValueChanged;

        public DropDown(string name, string text, List<string> elements, int defaultIndex, Action<int> OnValueChanged)
        {
            if (Manager.settingsElements.Keys.Contains(name)) return;

            this.name = name;
            this.text = text;
            this.elements = elements;
            this.defaultIndex = defaultIndex;
            this.OnValueChanged = OnValueChanged;

            BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<K8Lib>().StartCoroutine(createElement(name, text, elements, defaultIndex, OnValueChanged));
        }

        internal IEnumerator createElement(string name, string text, List<string> elements, int defaultIndex, Action<int> OnValueChanged)
        {
            start:

            while (!GameManager.GM) yield return null;

            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            foreach (string element in elements)
            {
                options.Add(new Dropdown.OptionData(element));
            }

            GameObject applicationSettingsMenu = GameManager.GM.GetComponent<ac_OptionsMenu>().OptionScreens[3].gameObject;

            while (!applicationSettingsMenu.activeSelf)
            {
                yield return null;
                if (applicationSettingsMenu == null) goto start;
            }

            ac_OptionsMenu optionsMenu = GameManager.GM.GetComponent<ac_OptionsMenu>();
            GameObject graphicsMenu = GameManager.GM.GetComponent<ac_OptionsMenu>().OptionScreens[2].gameObject;


            GameObject resolutionPicker = graphicsMenu.transform.GetChild(2).gameObject;


            GameObject referenceChild = resolutionPicker.transform.GetChild(1).gameObject;
            GameObject dropdown = GameObject.Instantiate(resolutionPicker, applicationSettingsMenu.transform, false);
            dropdown.name = name;
            dropdown.transform.localPosition = new Vector3(0, 75 - (50 * (Manager.settingsElements.Count)), 0);
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
            dropdownComponent.value = defaultIndex;

            dropdownComponent.onValueChanged.RemoveAllListeners();
            dropdownComponent.onValueChanged.AddListener(new UnityAction<int>(OnValueChanged));
            dropdownComponent.onValueChanged.AddListener((int value) =>
            {
                defaultIndex = value;
            });

            gameObject = dropdown;


            Manager.settingsElements.Add(name, this);
        }
    }

    public class TextInput
    {
        public GameObject gameObject;

        internal string name;
        internal string text;
        internal string existingText;
        internal string placeholderText;
        internal Action<string>? OnSubmit;
        internal Action<string>? OnValueChange;

        public TextInput(string name, string text, string existingText, string placeholderText, Action<string>? OnSubmit, Action<string>? OnValueChange = null)
        {
            if (Manager.settingsElements.Keys.Contains(name)) return;

            this.name = name;
            this.text = text;
            this.existingText = existingText;
            this.placeholderText = placeholderText;
            this.OnSubmit = OnSubmit;
            this.OnValueChange = OnValueChange;

            BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<K8Lib>().StartCoroutine(createElement(name, text, existingText, placeholderText, OnSubmit, OnValueChange));
        }

        internal IEnumerator createElement(string name, string text, string existingText, string placeholderText, Action<string>? OnSubmit, Action<string>? OnValueChange)
        {
            start:

            while (!GameManager.GM) yield return null;

            GameObject applicationSettingsMenu = GameManager.GM.GetComponent<ac_OptionsMenu>().OptionScreens[3].gameObject;

            while (!applicationSettingsMenu.activeSelf)
            {
                yield return null;
                if (applicationSettingsMenu == null) goto start;
            }

            GameObject inGameTextInput = applicationSettingsMenu.transform.GetChild(2).gameObject;

            GameObject textInput = GameObject.Instantiate(inGameTextInput, applicationSettingsMenu.transform, false);
            textInput.name = name;
            textInput.transform.localPosition = new Vector3(0, 225 - (50 * (Manager.settingsElements.Count + 3)), 0);

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

            if (OnSubmit != null) inputComponent.onSubmit.AddListener(new UnityAction<string>(OnSubmit));
            if (OnValueChange != null) inputComponent.onValueChanged.AddListener(new UnityAction<string>(OnValueChange));

            if (OnValueChange != null)
            {
                inputComponent.onValueChanged.AddListener((string value) =>
                {
                    existingText = value;
                });
            }

            if (OnSubmit != null)
            {
                inputComponent.onSubmit.AddListener((string value) =>
                {
                    OnSubmit(value);
                });
            }

            gameObject = textInput;

            Manager.settingsElements.Add(name, this);
        }
    }
}