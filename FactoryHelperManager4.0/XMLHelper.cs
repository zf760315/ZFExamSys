using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace FactoryHelperManager
{
    public class XMLHelper
    {
        /// <summary>
        /// 根据节点名称查找所有节点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeName"></param>
        /// <param name="findNodes"></param>
        public static void GetChildNodes(XmlNode node, string nodeName, ref List<XmlNode> findNodes)
        {
            foreach (XmlNode node1 in node.ChildNodes)
            {
                if (node1.Name.ToLower().Replace(" ", "") == nodeName.Replace(" ", "").ToLower())
                    findNodes.Add(node1);
                else
                {
                    if (node1.ChildNodes.Count >= 1)
                    {
                        GetChildNodes(node1, nodeName, ref findNodes);
                    }
                }
            }
        }

        /// <summary>
        /// 根据节点名称查找唯一节点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeName"></param>
        public static XmlNode GetSingleChildNodes(XmlNode node, string nodeName)
        {
            XmlNode xmlNode = null;
            FindSingleChildNodes(node, nodeName, ref xmlNode);
            return xmlNode;
        }

        public static void FindSingleChildNodes(XmlNode node, string nodeName, ref XmlNode xmlNode)
        {
            if (node != null && node.ChildNodes.Count > 0)
            {
                foreach (XmlNode node1 in node.ChildNodes)
                {
                    if (node1.Name.ToLower().Replace(" ", "") == nodeName.Replace(" ", "").ToLower())
                    {
                        xmlNode = node1;
                        return;
                    }
                    else
                    {
                        if (node1.ChildNodes.Count >= 1)
                        {
                            FindSingleChildNodes(node1, nodeName, ref xmlNode);
                        }
                    }
                }
            }
        }
    }
}
