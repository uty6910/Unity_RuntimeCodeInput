using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RuntimeCodeInputSystem : MonoBehaviour
{

    public ScrollRect _intelliSenseScorll;

    public InputField _inputField;

    private List<string> _intellisenseResultList = new List<string>();

    private List<menuActivate> _menuList = new List<menuActivate>();

    private string _previousString = "";

    public Canvas _canvas;

    private bool isOn = true;

    public GameObject prefab;

    public Text result;

    public int currentIndex = -1;

    public void test()
    {
        Debug.Log("sad");
    }

    public string test(string a, string b, float c)
    {
        return a + b + c;
    }

    public Vector3 test(Vector3 vector, string b, Vector3 vector2)
    {
        return vector + vector2;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            isOn = !isOn;
            if (isOn)
            {
                _canvas.gameObject.SetActive(isOn);
                _inputField.ActivateInputField();
            }
        }

        _canvas.gameObject.SetActive(isOn);

        if (!isOn) return;

        if (Input.GetKeyDown(KeyCode.Period))
        {
            _intellisenseResultList.Clear();
            currentIndex = -1;
            if (_inputField.text.Split('.').Length > 0)
            {
                Intellisense(_inputField.text.Split('.'));
                _previousString = _inputField.text;
            }
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) && _intelliSenseScorll.gameObject.activeSelf)
        {
            EventSystem.current.SetSelectedGameObject(null);
            int previousIndex = currentIndex;
            for (int i = 0; i < _menuList.Count; i++)
            {
                if (_menuList[i].gameObject.activeSelf && i != currentIndex && currentIndex < i)
                {
                    currentIndex = i;
                    _menuList[i].Activate = true;
                    SetScrollPosition(i);
                    break;
                }
                else if (_menuList[i].gameObject.activeSelf) _menuList[i].Activate = false;
            }
            if (previousIndex == currentIndex) _menuList[currentIndex].Activate = true;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && _intelliSenseScorll.gameObject.activeSelf && currentIndex > 0)
        {
            EventSystem.current.SetSelectedGameObject(null);
            if (currentIndex == 0)
            {
                _menuList[currentIndex].Activate = false;
                currentIndex = -1;
                _inputField.ActivateInputField();
                return;
            }
            for (int i = currentIndex; i > -1; i--)
            {
                if (_menuList[i].gameObject.activeSelf && i != currentIndex)
                {
                    currentIndex = i;
                    _menuList[i].Activate = true;
                    SetScrollPosition(i);
                    break;
                }
                else if (_menuList[i].gameObject.activeSelf) _menuList[i].Activate = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (currentIndex != -1)
            {
                IntellisenseTab(_inputField.text);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _inputField.ActivateInputField();
            if (_intelliSenseScorll.gameObject.activeSelf)
            {
                if (currentIndex != -1)
                    _menuList[currentIndex].Activate = false;
                currentIndex = -1;
            }
            _intelliSenseScorll.gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Result(_inputField.text.Split('.'));
        }

        _inputField.text.Split('.');

        if (_inputField.text == "")
        {
            _intelliSenseScorll.gameObject.SetActive(false);
        }

        if (_previousString != _inputField.text && _previousString != "")
        {
            SetContent(_inputField.text);
            _previousString = _inputField.text;
        }
    }

    private void SetScrollPosition(int index)
    {
        var _rectTransform = _menuList[index].GetComponent<RectTransform>();

        float _rectPosition = -_rectTransform.anchoredPosition.y - (_rectTransform.rect.height * (1 - _rectTransform.pivot.y));

        float _rectHeight = _rectTransform.rect.height;

        float _scrollHeight = _intelliSenseScorll.GetComponent<RectTransform>().rect.height;

        float _scrollAnchorPosition = _intelliSenseScorll.content.anchoredPosition.y;

        float _offlimitsValue = GetScrollOffset(_rectPosition, _scrollAnchorPosition, _rectHeight, _scrollHeight);

        _intelliSenseScorll.verticalNormalizedPosition +=
            (_offlimitsValue / _intelliSenseScorll.content.rect.height) * Time.unscaledDeltaTime * 100;
    }

    private float GetScrollOffset(float position, float anchorPosition, float rectHeight, float scrollHeight)
    {
        if (position < anchorPosition + (rectHeight / 2))
        {
            return (anchorPosition + scrollHeight) - (position - rectHeight);
        }
        else if (position + rectHeight > anchorPosition + scrollHeight)
        {
            return (anchorPosition + scrollHeight) - (position + rectHeight);
        }
        return 0;
    }

    private void ResetContent()
    {
        var textList = _intelliSenseScorll.content.GetComponentsInChildren<Text>(true);
        _intelliSenseScorll.gameObject.SetActive(false);
        foreach (var text in textList)
        {
            text.gameObject.SetActive(false);
            text.text = "";
        }
    }

    private void SetContent()
    {
        _intelliSenseScorll.gameObject.SetActive(true);
        var rect = _intelliSenseScorll.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(110 + _inputField.caretPosition * 5, rect.anchoredPosition.y);

        Transform parent = _intelliSenseScorll.content;

        var textList = parent.GetComponentsInChildren<Text>(true);

        if (_intellisenseResultList.Count > textList.Length)
        {
            for (int i = 0; i < _intellisenseResultList.Count; i++)
            {
                if (i < textList.Length)
                {
                    textList[i].gameObject.SetActive(true);
                    textList[i].text = _intellisenseResultList[i];
                }
                else
                {
                    var ob = Instantiate(prefab, parent);
                    ob.GetComponent<Text>().text = _intellisenseResultList[i];
                    _menuList.Add(ob.GetComponent<menuActivate>());
                }
            }
        }
        else
        {
            for (int i = 0; i < textList.Length; i++)
            {
                if (i < _intellisenseResultList.Count)
                {
                    textList[i].gameObject.SetActive(true);
                    textList[i].text = _intellisenseResultList[i];
                }
                else textList[i].gameObject.SetActive(false);
            }
        }
    }

    private void SetContent(string _inputText)
    {
        var text = _inputText.Split('.')[_inputText.Split('.').Length - 1];

        Transform parent = _intelliSenseScorll.content;

        var textList = parent.GetComponentsInChildren<Text>(true);

        if (text.Contains(')')) text = text.Replace(")", "");

        if (text.Contains("<"))
        {
            _intellisenseResultList.Clear();
            SetComponent(_inputText);
        }
        else
        {
            for (int i = 0; i < textList.Length; i++)
            {
                if (i < _intellisenseResultList.Count)
                {
                    if (!_intellisenseResultList[i].ToUpper().Contains(text.ToUpper()) || !textList[i].text.ToUpper().Contains(text.ToUpper()))
                        textList[i].gameObject.SetActive(false);
                    else
                        textList[i].gameObject.SetActive(true);
                }
            }
        }
    }
    private void IntellisenseTab(string name)
    {
        var names = name.Split('.');
        var text = names[names.Length - 1];
        var componentText = text.Split('<')[0];

        if (text.Contains("<") && name.Split('<').Length - 1 < 2)
        {
            if(_inputField.text.Split('<')[1] != "") _inputField.text = _inputField.text.Replace(text, componentText + "<");
            _inputField.text += _intellisenseResultList[currentIndex] +">()";
            _previousString = "";
            if (_menuList[currentIndex] != null && currentIndex != -1)
                _menuList[currentIndex].Activate = false;
            currentIndex = -1;
            _intelliSenseScorll.gameObject.SetActive(false);
            _inputField.ActivateInputField();
        }
        else
        {
            var ob = GameObject.Find(names[0]);

            Type type = ob.GetType();
            bool isMethod = false;
            object _ob = null;
            string SearchResult = TypeUtility.SearchGetType(ob, _inputField.text, names, ref type, ref _ob);

            if (SearchResult == "GameObject" && type == null)
            {
                isMethod = true;
            }
            else
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);


                for (int i = 0; i < methods.Length; i++)
                {
                    if (methods[i].Name == _intellisenseResultList[currentIndex])
                    {
                        isMethod = true;
                        break;
                    }
                }
            }
            names = name.Split('.');
            _inputField.text = "";
            string period = ".";

            if (isMethod && !_intellisenseResultList[currentIndex].Contains("GetComponent"))
            {
                names[names.Length - 1] = names[names.Length - 1].Replace(names[names.Length - 1].Split(')')[0], _intellisenseResultList[currentIndex] + "()");
            }
            else if(isMethod && type == null && SearchResult == "GameObject")
            {
                names[names.Length - 1] = _intellisenseResultList[currentIndex] +")";
            }
            else if (!isMethod && type != null && SearchResult == "GameObject" && text.Contains("<"))
            {
                names[names.Length - 1] = componentText + "<" + _intellisenseResultList[currentIndex] + ">())";
            }
            else if (isMethod && type != null && SearchResult == "GameObject" && !_intellisenseResultList[currentIndex].Contains("GetComponent"))
            {
                names[names.Length - 1] = _intellisenseResultList[currentIndex] + "())";
            }
            else if (isMethod && type != null && SearchResult == "GameObject" && _intellisenseResultList[currentIndex].Contains("GetComponent"))
            {
                names[names.Length - 1] = _intellisenseResultList[currentIndex] + ")";
            }
            else if (!isMethod && type != null && SearchResult == "GameObject")
            {
                names[names.Length - 1] = _intellisenseResultList[currentIndex] + ")";
            }
            else
            {
                names[names.Length - 1] = _intellisenseResultList[currentIndex];
            }

            for (int i = 0; i < names.Length; i++)
            {
                if (i >= names.Length - 1) period = "";
                _inputField.text += names[i] + period;
            }

            _intelliSenseScorll.gameObject.SetActive(false);
            if (_menuList[currentIndex] != null && currentIndex != -1)
                _menuList[currentIndex].Activate = false;
            currentIndex = -1;
            _inputField.ActivateInputField();
            _inputField.MoveTextEnd(true);
        }
    }

    private void SetComponent(string name)
    {
        string[] names = name.Split('.');
        var ob = GameObject.Find(names[0]);
  

        if (name.Split('<').Length - 1 < 2)
        {
            Component[] components = null;
            if (names[1].Contains("Children"))
                components = ob.GetComponentsInChildren(typeof(Component));
            else if (names[1].Contains("Parent"))
                components = ob.GetComponentsInParent(typeof(Component));
            else components = ob.GetComponents(typeof(Component));

            foreach (var com in components)
            {
                var componentName = com.ToString().Split('(')[1];

                componentName = componentName.Replace(")", "");

                if (componentName.Split('.').Length != 0)
                {
                    componentName = componentName.Split('.')[componentName.Split('.').Length - 1];
                }
                _intellisenseResultList.Add(componentName);
            }
        }
        else
        {
            int tempindex = 1;
            int targetCount = 0;
            Type type = ob.GetType();
            object TargetObject = ob;
            Component[] components = null;
            if (names[1].Contains("Children"))
                components = ob.GetComponentsInChildren(typeof(Component));
            else if (names[1].Contains("Parent"))
                components = ob.GetComponentsInParent(typeof(Component));
            else components = ob.GetComponents(typeof(Component));

            while (tempindex != names.Length)
            {
                if (names[tempindex].Contains("GetComponent"))
                {
                    foreach (var com in components)
                    {
                        if (com.ToString().Contains(TypeUtility.GetMiddleString(names[tempindex], "<", ">")))
                        {
                            targetCount++;
                            if (targetCount == name.Split('<').Length - 1)
                            {
                                var _ob = TargetObject as GameObject;
                                var _components = _ob.GetComponents(typeof(Component));

                                foreach (var _com in _components)
                                {
                                    var componentName = _com.ToString().Split('(')[1];

                                    componentName = componentName.Replace(")", "");

                                    if (componentName.Split('.').Length != 0)
                                    {
                                        componentName = componentName.Split('.')[componentName.Split('.').Length - 1];
                                    }
                                    _intellisenseResultList.Add(componentName);
                                }
                            }
                            else
                            {
                                TargetObject = com;
                                type = com.GetType();
                            }
                            break;
                        }
                    }
                }

                if (!names[tempindex].Contains("GetComponent") && names[tempindex].Contains("("))
                {
                    var tempArgs = TypeUtility.SetMethodSplit(ref tempindex, ref names, name);

                    List<string> argStringList = TypeUtility.SetArgument(ref tempindex, ref names, name, tempArgs);

                    if (argStringList[0].Contains("GameObject") && argStringList.Count == 1)
                    {

                        tempArgs = argStringList[0].Split('.');

                        string obName = (tempArgs.Length > 1) ? TypeUtility.GetMiddleString(tempArgs[1], "(", ")") : argStringList[0];

                        var _ob = GameObject.Find(obName);

                        Type _type = ob.GetType();

                        object a = null;

                        if (tempArgs.Length > 1 && tempArgs[1] != "")
                        {
                            string _original = obName + argStringList[0].Replace(tempArgs[0] + "." + tempArgs[1], "");

                            if (_original != obName)
                            {
                                var _split = _original.Split('.');

                                TypeUtility.SearchGetType(_ob, _original, _split, ref _type, ref a);
                            }
                            type = _type;
                        }
                        names = name.Split('.');
                        tempindex--;
                        type = _type;
                        TargetObject = _ob;
                    }

                }

                var members = type.GetMember(names[tempindex]);

                for (int i = 0; i < members.Length; i++)
                {
                    if (members[i].Name == names[tempindex])
                    {
                        if (members[i].MemberType == MemberTypes.Field)
                        {
                            TargetObject = ((FieldInfo)members[i]).GetValue(TargetObject);
                            type = ((FieldInfo)members[i]).FieldType;
                            break;
                        }
                        else if (members[i].MemberType == MemberTypes.Property)
                        {
                            TargetObject = ((PropertyInfo)members[i]).GetValue(TargetObject, null);
                            type = ((PropertyInfo)members[i]).PropertyType;
                            break;
                        }
                    }
                }

                tempindex++;
            }
        }
        _intelliSenseScorll.gameObject.SetActive(true);
        var rect = _intelliSenseScorll.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(110 + _inputField.caretPosition + 5, rect.anchoredPosition.y);

        Transform parent = _intelliSenseScorll.content;

        var textList = parent.GetComponentsInChildren<Text>(true);

        for (int i = 0; i < textList.Length; i++)
        {
            if (i < _intellisenseResultList.Count)
            {
                textList[i].gameObject.SetActive(true);
                textList[i].text = _intellisenseResultList[i];
            }
            else textList[i].gameObject.SetActive(false);
        }
    }

    private void Result(string[] name)
    {
        if (name[0] == "") return;
        var ob = GameObject.Find(name[0]);

        Type type = ob.GetType();

        object _ob = null;

        result.text = TypeUtility.SearchGetType(ob, _inputField.text, name, ref type, ref _ob);
    }

    public void Intellisense(string[] name)
    {
        var ob = GameObject.Find(name[0]);

        if (ob != null && name.Length == 2)
        {
            var properties = ob.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            for (int i = 0; i < properties.Length; i++)
            {
                if (!_intellisenseResultList.Contains((properties[i]).Name))
                    _intellisenseResultList.Add((properties[i]).Name);
            }

            var members = ob.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            for (int i = 0; i < members.Length; i++)
            {
                if (!_intellisenseResultList.Contains((members[i]).Name))
                    _intellisenseResultList.Add((members[i]).Name);
            }
            var methods = ob.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            for (int i = 0; i < methods.Length; i++)
            {
                if (!_intellisenseResultList.Contains((methods[i]).Name))
                    _intellisenseResultList.Add((methods[i]).Name);
            }
            SetContent();
        }
        else if (ob != null)
        {

            Type type = ob.GetType();

            object _ob = null;

            if (TypeUtility.SearchGetType(ob, _inputField.text, name, ref type, ref _ob) == "GameObject" && type == null)
            {
                var methods = TypeUtility.GetTypeByName("GameObject").GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                for (int i = 0; i < methods.Length; i++)
                {
                    if (!_intellisenseResultList.Contains((methods[i]).Name))
                        _intellisenseResultList.Add((methods[i]).Name);
                }
            }
            else if (type != null)
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < properties.Length; i++)
                {
                    if (!_intellisenseResultList.Contains((properties[i]).Name))
                        _intellisenseResultList.Add((properties[i]).Name);
                }

                var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < members.Length; i++)
                {
                    if (!_intellisenseResultList.Contains((members[i]).Name))
                        _intellisenseResultList.Add((members[i]).Name);
                }
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                for (int i = 0; i < methods.Length; i++)
                {
                    if (!_intellisenseResultList.Contains((methods[i]).Name))
                        _intellisenseResultList.Add((methods[i]).Name);
                }
            }
            SetContent();
        }   
    }
}
