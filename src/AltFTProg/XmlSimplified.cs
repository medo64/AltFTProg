namespace AltFTProg;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

internal class XmlSimplified {

    public XmlSimplified(FileInfo file) {
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(file.FullName);

        var chipType = default(string);
        var properties = new List<KeyValuePair<string, string>>();
        TraverseNodes(xmlDoc.DocumentElement, properties);
        for (var i = 0; i < properties.Count; i++) {
            if (properties[i].Key.Equals("Chip_Details/Type", StringComparison.OrdinalIgnoreCase)) {
                chipType = properties[i].Value;
                properties.RemoveAt(i);
                break;
            }
        }

        ChipType = chipType ?? "";
        Properties = properties.AsReadOnly();
    }

    static void TraverseNodes(XmlNode? node, List<KeyValuePair<string, string>> properties) {
        if (node == null) { return; }
        if (node.NodeType == XmlNodeType.Text) {
            if (node.ParentNode == null) { return; }
            var parentName = node.ParentNode.ParentNode?.Name ?? "";
            var name = node.ParentNode.Name;
            var value = node.Value;
            if (string.IsNullOrEmpty(value)) { return; }
            properties.Add(new KeyValuePair<string, string>(parentName + "/" + name, value));
        }

        foreach (XmlNode child in node.ChildNodes) {
            TraverseNodes(child, properties);
        }
    }


    public string ChipType { get; }
    public ReadOnlyCollection<KeyValuePair<string, string>> Properties { get; }

    public bool IsMatchingDevice(FtdiDevice device) {
        if (device is Ftdi232RDevice) {
            return ChipType.Equals("FT232R", StringComparison.OrdinalIgnoreCase);
        } else if (device is FtdiXSeriesDevice) {
            return ChipType.Equals("FT X Series", StringComparison.OrdinalIgnoreCase);
        } else {
            return false;
        }
    }

}