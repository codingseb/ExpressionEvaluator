﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré par un outil.
//     Version du runtime :4.0.30319.42000
//
//     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//     le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CodingSeb.ExpressionEvaluator.Tests {
    using System;
    
    
    /// <summary>
    ///   Une classe de ressource fortement typée destinée, entre autres, à la consultation des chaînes localisées.
    /// </summary>
    // Cette classe a été générée automatiquement par la classe StronglyTypedResourceBuilder
    // à l'aide d'un outil, tel que ResGen ou Visual Studio.
    // Pour ajouter ou supprimer un membre, modifiez votre fichier .ResX, puis réexécutez ResGen
    // avec l'option /str ou régénérez votre projet VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Retourne l'instance ResourceManager mise en cache utilisée par cette classe.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CodingSeb.ExpressionEvaluator.Tests.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Remplace la propriété CurrentUICulture du thread actuel pour toutes
        ///   les recherches de ressources à l'aide de cette classe de ressource fortement typée.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à /* Script0001 */
        ///x = 0;
        ///result = &quot;&quot;;
        ///
        ///while(x &lt; 5)
        ///{
        ///	result += $&quot;{x},&quot;;
        ///	x++;
        ///}
        ///
        ///result.Remove(result.Length - 1);.
        /// </summary>
        internal static string Script0001 {
            get {
                return ResourceManager.GetString("Script0001", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à /* Script0002 */
        ///result = &quot;&quot;;
        ///
        ///for(x = 0; x &lt; 5;x++)
        ///{
        ///	result += $&quot;{x},&quot;;
        ///}
        ///
        ///result.Remove(result.Length - 1);.
        /// </summary>
        internal static string Script0002 {
            get {
                return ResourceManager.GetString("Script0002", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à /* Script0003 */
        ///result = &quot;&quot;;
        ///
        ///for(x = 0; x &lt; 5;x++)
        ///	result += $&quot;{x},&quot;;
        ///
        ///result.Remove(result.Length - 1);.
        /// </summary>
        internal static string Script0003 {
            get {
                return ResourceManager.GetString("Script0003", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à /* Script0004 */
        ///x = [valx];
        ///y = [valy];
        ///result = 0;
        ///
        ///if(y != 0)
        ///{
        ///	result = 1;
        ///}
        ///else if(x == 0)
        ///{
        ///	result = 2;
        ///}
        ///else if(x &lt; 0)
        ///{
        ///	result = 3;
        ///}
        ///else
        ///{
        ///	result = 4;
        ///}
        ///
        ///result;.
        /// </summary>
        internal static string Script0004 {
            get {
                return ResourceManager.GetString("Script0004", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à /* Script0005 */
        ///x = [valx];
        ///y = [valy];
        ///result = 0;
        ///
        ///if(y != 0)
        ///	result = 1;
        ///else if(x == 0)
        ///	result = 2;
        ///else if(x &lt; 0)
        ///	result = 3;
        ///else
        ///	result = 4;
        ///
        ///result;.
        /// </summary>
        internal static string Script0005 {
            get {
                return ResourceManager.GetString("Script0005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à /* Script0006 */
        ///List(1,2,3,4)
        ///    .Find((element) =&gt; 
        ///    {
        ///        x = 2 + 2;
        ///        element == 3;
        ///    });.
        /// </summary>
        internal static string Script0006 {
            get {
                return ResourceManager.GetString("Script0006", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à /* Script0007 */
        ///x = 4;
        ///
        ///if(x == 4)
        ///	List(1,2,3,4)
        ///		.Find((element) =&gt; 
        ///		{
        ///			x = 2 + 2;
        ///			element == 3;
        ///		});.
        /// </summary>
        internal static string Script0007 {
            get {
                return ResourceManager.GetString("Script0007", resourceCulture);
            }
        }
    }
}