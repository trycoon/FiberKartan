using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

/*
The zlib/libpng License
Copyright (c) 2012 Henrik Östman

This software is provided 'as-is', without any express or implied warranty. In no event will the authors be held liable for any damages arising from the use of this software.
Permission is granted to anyone to use this software for any purpose, including commercial applications, and to alter it and redistribute it freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/
namespace FiberKartan
{
    public partial class ResultBox : System.Web.UI.UserControl
    {
        // Declare variables
        private string _ResultMessage;
        private Unit _Width;
        private Unit _Height;
        private int _BorderWidth;
        private Color _BorderColor;
        private Color _BackgroundColor;
        private Color _FontColor;
        private bool _FontBold;
        private ThemeChoices _ThemeChoices;


        public ResultBox()
        {
            this.Visible = false; // Don't show box by default.
        }

        /// <summary>
        /// Gets or sets the message to display.
        /// </summary>
        public string Text
        {
            get { return _ResultMessage; }
            set
            {
                _ResultMessage = value;
                lblError.Text = _ResultMessage;
                this.Visible = true;    // When setting a message we assume the box should be visible.
            }
        }

        /// <summary>
        /// Gets or sets the error message box width. Value cannot be less than 40 pixels or greater than 800 pixels.
        /// </summary>
        public Unit BoxWidth
        {
            get { return _Width; }
            set
            {
                _Width = value;

                if (_Width.Value > 800)
                {
                    throw new ApplicationException("Message box width cannot be greater than 800 pixels.");
                }
                else if (_Width.Value < 40)
                {
                    throw new ApplicationException("Message box width cannot be less than 40 pixels.");
                }
                else
                {
                    msgTable.Width = _Width;
                }
            }
        }

        /// <summary>
        /// Gets or sets the error message box height. Value cannot be less than 10 pixels.
        /// </summary>
        public Unit BoxHeight
        {
            get { return _Height; }
            set
            {
                _Height = value;

                if (_Height.Value < 10)
                {
                    throw new ApplicationException("Message box height cannot be less than 20 pixels.");
                }
                else
                {
                    msgTable.Height = _Height;
                }
            }
        }

        /// <summary>
        /// Gets or sets the error message box border width. Default border value is 1.
        /// </summary>
        public int BorderWidth
        {
            get { return _BorderWidth; }
            set
            {
                _BorderWidth = value;
                msgTable.BorderWidth = _BorderWidth;

                if (_BorderColor == null)
                {
                    _BorderColor = Color.Red;
                }
            }
        }

        /// <summary>
        /// Gets or sets the error message box border color. Default value is red.
        /// </summary>
        public Color BorderColor
        {
            get { return _BorderColor; }
            set
            {
                _BorderColor = value;
                msgTable.BorderColor = _BorderColor;
                if (_BorderWidth == 0)
                {
                    msgTable.BorderWidth = 1;
                }
            }
        }

        /// <summary>
        /// Gets or sets the error message box background color. Default color value is salmon.
        /// </summary>
        public Color BackgroundColor
        {
            get { return _BackgroundColor; }
            set
            {
                _BackgroundColor = value;
                msgTable.BackColor = _BackgroundColor;
            }
        }

        /// <summary>
        /// Gets or sets the error message box font color. Default color value is red.
        /// </summary>
        public Color FontColor
        {
            get { return _FontColor; }
            set
            {
                _FontColor = value;
                lblError.ForeColor = _FontColor;
            }
        }

        /// <summary>
        /// Gets or sets the error message box font weight. Default value is bold.
        /// </summary>
        public bool FontBold
        {
            get { return _FontBold; }
            set
            {
                _FontBold = value;
                lblError.Font.Bold = _FontBold;
            }
        }

        // Enum for theme options
        public enum ThemeChoices
        {
            ErrorTheme,
            InformationTheme
        }

        /// <summary>
        /// Gets or sets the error box theme as a shortcut to specifying other attributes.
        /// </summary>
        public ThemeChoices BoxTheme
        {
            get { return _ThemeChoices; }
            set
            {
                _ThemeChoices = value;

                switch (_ThemeChoices)
                {
                    case ThemeChoices.ErrorTheme:
                        // Error theme
                        msgTable.BackColor = Color.FromArgb(254, 244, 241);
                        msgTable.BorderWidth = 1;
                        msgTable.BorderColor = Color.Red;
                        lblError.ForeColor = Color.Red;
                        break;
                    case ThemeChoices.InformationTheme:
                        // Information theme
                        msgTable.BackColor = Color.FromArgb(255, 255, 233);
                        msgTable.BorderWidth = 1;
                        msgTable.BorderColor = Color.Black;
                        lblError.ForeColor = Color.Black;
                        break;
                }
            }
        }
    }
}