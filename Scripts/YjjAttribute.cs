using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YjjAttribute
{
}
public class FolderAttribute : Attribute
{
    public string name;
    public FolderAttribute(string title)
    {
        name = title;
    }
}
public class FolderEndAttribute : Attribute
{

}
public class MyInspectorAttribute : Attribute
{
    public string name;
    public string depend = "";
    public MyInspectorAttribute(string title = "",string depen = "")
    {
        name = title;
        depend = depen;
    }
}
