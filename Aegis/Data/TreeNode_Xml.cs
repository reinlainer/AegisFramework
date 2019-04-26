using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json.Linq;



namespace Aegis.Data
{
    public partial class TreeNode<T>
    {
        public static TreeNode<string> LoadFromXml(string xml, string nodeName)
        {
            TreeNode<string> root = new TreeNode<string>(null, nodeName, null);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);


            XmlNode node = xmlDoc.SelectSingleNode(nodeName);
            Xml_GetChilds(node, root);

            return root;
        }


        private static void Xml_GetAttributes(XmlNode node, TreeNode<string> target)
        {
            foreach (XmlAttribute attr in node.Attributes)
            {
                new TreeNode<string>(target, attr.Name, attr.Value);
            }
        }


        private static void Xml_GetChilds(XmlNode node, TreeNode<string> target)
        {
            Xml_GetAttributes(node, target);
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Comment)
                    continue;

                TreeNode<string> newNode = new TreeNode<string>(target, child.Name, null);
                Xml_GetChilds(child, newNode);
            }
        }


        public static void TreeNodeToJson(TreeNode<string> node, JObject target)
        {
            JObject jsonChild = target;
            if (node.Name != "")
            {
                if (node.Value != null)
                    target.Add(node.Name, node.Value);
                else
                {
                    jsonChild = new JObject();
                    target.Add(node.Name, jsonChild);
                }
            }

            foreach (var childNode in node.Childs)
                TreeNodeToJson(childNode, jsonChild);
        }


        public static TreeNode<string> JsonToTreeNode(JObject json, TreeNode<string> target)
        {
            TreeNode<string> node = null;
            foreach (var data in json)
            {
                if (data.Value is JValue)
                    new TreeNode<string>(target, data.Key, data.Value.ToString());
                else
                {
                    node = new TreeNode<string>(target, data.Key, null);
                    JsonToTreeNode(data.Value.ToObject<JObject>(), node);
                }
            }

            return node;
        }
    }
}
