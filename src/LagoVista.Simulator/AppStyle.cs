using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Simulator
{
    class AppStyle : IAppStyle
    {
        public Color TitleBarBackground => NamedColors.NuvIoTWhite;

        public Color TitleBarText => NamedColors.NuvIoTBlack;

        public Color PageBackground => NamedColors.White;

        public Color PageText => Color.CreateColor(0x33, 0x33, 0x33);

        // public Color LabelText => Color.CreateColor(0x80, 0x80, 0x80);
        public Color LabelText => NamedColors.Black;

        public Color EditControlBackground => NamedColors.White;

        public Color EditControlText => NamedColors.NuvIoTBlack;

        public Color EditControlFrame => Color.CreateColor(0xCC, 0xCC, 0xCC);

        public Color EditControlFrameFocus => NamedColors.NuvIoTBlack;

        public Color EditControlFrameInvalid => NamedColors.Red;

        public Color MenuBarBackground => Color.CreateColor(0x2E, 0x35, 0x3D);

        public Color MenuBarForeground => NamedColors.NuvIoTWhite;

        public Color MenuBarBackgroundActive => NamedColors.Black;

        public Color MenuBarForegroundActive => NamedColors.White;

        public Color ButtonBackground => NamedColors.NuvIoTDark;

        public Color ButtonBorder => NamedColors.NuvIoTBlack;

        public Color ButtonForeground => NamedColors.White;

        public Color ButtonBackgroundActive => NamedColors.NuvIoTWhite;

        public Color ButtonBorderActive => NamedColors.NuvIoTMedium;

        public Color ButtonForegroundActive => NamedColors.NuvIoTLight;

        public Color HighlightColor => NamedColors.NuvIoTContrast;

        public Color RowSeperatorColor => NamedColors.NuvIoTDark;

    }
}
