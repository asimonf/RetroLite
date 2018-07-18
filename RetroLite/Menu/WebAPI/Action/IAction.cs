using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xilium.CefGlue;

namespace RetroLite.Menu.WebAPI.Action
{
    public interface IAction
    {
        string Path { get; }
        string Method { get; }

        ApiResponse ProcessRequest(CefRequest request);
    }
}
