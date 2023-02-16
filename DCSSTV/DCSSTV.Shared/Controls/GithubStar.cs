using Microsoft.UI.Xaml;
using Uno.UI.Runtime.WebAssembly;

namespace DCSSTV.Controls
{
    [HtmlElement("iframe")]
    public sealed partial class GithubStar : FrameworkElement
    {
        public GithubStar() // Will create an <input> HTML element
        {
            this.SetHtmlAttribute("src", "https://ghbtns.com/github-btn.html?user=rytisgit&repo=dcssreplay&type=star&count=true");
            this.SetHtmlAttribute("frameborder", "0");
            this.SetHtmlAttribute("scrolling", "0");
            this.SetHtmlAttribute("width", "170");
            this.SetHtmlAttribute("height", "20");
            this.SetHtmlAttribute("title", "Star Rytisgit/DCSSReplay on GitHub");
            this.SetCssClass("github-button");
            this.SetHtmlContent("Star");
        }
    }
}
