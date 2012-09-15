using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Helper
{
    public class ChatMessage
    {
        /* Holds a single chat message and stores the information about it
         * Who its from
         * When it was recieved (this is now stored in the sorted list in chat as an index)
         * Any special coloring info? (What team they were on maybe)
         */

        public DateTime Time { get; set; }
        public String Owner { get; set; }
        public String Message { get; set; }
        public Color MessageColor { get; set; }

        public override string ToString()
        {
            return "[" + Time.ToLongTimeString() + "] " + Owner + ": " + Message;
        }
    }

    public class Chat
    {
        /* Class handles input parsing from a callback from InputManager
         * Class is owned by a Game
         * Class should draw the text with specified settings at a given location (ie, timeout for text, etc)
         * Allow for a "Console" mode that displays more text
         * Class should know if we are currently typing (thus allowing for displaying of the current text with an input cursor)
         *      This also should allow for callbacks to the mouse handler when implemented to allow for selection of where the input cursor is
         *      Otherwise keyboard input should manipulate it
         *      
         * Todo
         *      Add support for highlighting using shift, thus allowing sections to be overwritten
         */

        String currentMessage;
        int currentPosition;
        SortedList<long, ChatMessage> ChatLog;
        SpriteFont font;

        Texture2D inputBox;

        public int ChatLogLength { get; private set; } // Max length of ChatLog
        public int DisplayTime { get; private set; } // How long chat messages should be displayed for
        public int FadeOutTime { get; private set; } // How long after reaching DisplayLength it should take to fade out and no longer be displayed
        public bool Typing { get; set; } // If in type mode, display the chat box
        public Vector2 Position { get; set; }
        public String PlayerAlias { get; set; }

        /// <summary>
        /// Default Chat Constructor
        /// </summary>
        /// <param name="font">Font used to draw the text</param>
        public Chat(SpriteFont font) : this(font, new Vector2(20, 100), 100, 25, 5) { }

        /// <summary>
        /// Initializes the Chat component for a Game
        /// </summary>
        /// <param name="f">Font used to draw the text</param>
        /// <param name="start">Bottomw Left of the font window (text scrolls up)</param>
        /// <param name="logLength">Determines the Maximum number of messages to keep</param>
        /// <param name="display">Determines how long a message is displayed for</param>
        /// <param name="fade">Determines how long a message takes to fade out after 'display' time is elapsed</param>
        public Chat(SpriteFont f, Vector2 start, int logLength, int display, int fade)
        {
            font = f;
            Position = start;
            ChatLog = new SortedList<long, ChatMessage>();
            ChatLogLength = logLength;
            DisplayTime = display;
            FadeOutTime = fade;
            currentMessage = "";
        }

        public bool KeysPressed(List<Keys> pressed, out ChatMessage message)
        {
            message = null;
            bool exitMode = false;
            bool shift = pressed.Contains(Keys.LeftShift) || pressed.Contains(Keys.RightShift);
            foreach (Keys k in pressed)
            {
                if (k == Keys.Left)
                {
                    if (currentPosition > 0)
                        currentPosition--;
                }
                else if (k == Keys.Home)
                    currentPosition = 0;
                else if (k == Keys.Right)
                {
                    if (currentPosition < currentMessage.Length)
                        currentPosition++;
                }
                else if (k == Keys.End)
                    currentPosition = currentMessage.Length;
                else if (k == Keys.Enter)
                {
                    exitMode = true;
                    message = new ChatMessage();
                    message.Owner = PlayerAlias; // TODO: fix
                    message.Time = DateTime.Now;
                    message.Message = currentMessage;
                    message.MessageColor = Color.DarkBlue;

                    currentMessage = "";
                    currentPosition = 0;
                    // Add Message to list and send a packet about it
                    // let server add it for us
                    //ChatLog.Add(message.Time.Ticks, message);
                }
                else if (k == Keys.Escape)
                {
                    exitMode = true;
                    currentPosition = 0;
                    currentMessage = "";
                } // TODO, Backspace, Delete
                else if (k == Keys.Back)
                {
                    if (currentPosition > 0)
                    {
                        currentMessage = currentMessage.Remove(currentPosition - 1, 1);
                        currentPosition--;
                    }
                }
                else if (k == Keys.Delete)
                {
                    if (currentPosition < currentMessage.Length)
                    {
                        currentMessage = currentMessage.Remove(currentPosition, 1);
                    }
                }
                else if (k == Keys.LeftShift || k == Keys.RightShift
                    || k == Keys.LeftControl || k == Keys.RightControl
                    || k == Keys.LeftAlt || k == Keys.RightAlt)
                {
                    // Special characters handled elsewhere or not at all
                }
                else
                {
                    ProcessText(k, shift);
                    //currentMessage = currentMessage.Insert(currentPosition, KeyToText(k));
                    //currentPosition += k.ToString().Length;
                }
            }
            return exitMode;
        }

        public void Draw(SpriteBatch sb)
        {
            //Assume sb.Start is called already, because we cannot reset the state of the Graphics Device here
            if (Typing) // Draw chat box for input and cursor
            {
                if(inputBox == null)
                {
                    inputBox = new Texture2D(sb.GraphicsDevice, 1, 1);
                    inputBox.SetData(new Color[] { Color.White});
                }
                int size = Math.Max(100, (int)font.MeasureString(currentMessage + " ").X);
                sb.Draw(inputBox, new Rectangle((int)Position.X, (int)Position.Y, size, font.LineSpacing), Color.WhiteSmoke * .5f);
                Vector2 temp = font.MeasureString(currentMessage.Substring(0, currentPosition));
                sb.Draw(inputBox, new Rectangle((int)(Position.X + temp.X), (int)(Position.Y), 1, font.LineSpacing), Color.Black);
            }
            long cutoff = DateTime.Now.Ticks - (TimeSpan.TicksPerSecond * DisplayTime);

            Vector2 pos = Position;

            sb.DrawString(font, currentMessage, pos, Color.Black);
            pos.Y -= font.LineSpacing;

            int i = ChatLog.Count - 1;
            while(i >= 0 && ChatLog.Keys[i] >= cutoff)
            {
                sb.DrawString(font, ChatLog.Values[i].ToString(), pos, ChatLog.Values[i].MessageColor);
                pos.Y -= font.LineSpacing;
                i--;
            }

            long fadeTicks = TimeSpan.TicksPerSecond * FadeOutTime;
            long fadeout = cutoff - fadeTicks;

            while(i >= 0 && ChatLog.Keys[i] >= fadeout)
            {
                 // Messy way to copy
                sb.DrawString(font, ChatLog.Values[i].ToString(), pos, ChatLog.Values[i].MessageColor * ((float)(ChatLog.Keys[i] - fadeout) / (float)fadeTicks));
                pos.Y -= font.LineSpacing;
                i--;
            }
        }

        public void ProcessText(Keys k, bool shiftHeld)
        {
            String s = k.ToString();
            if (k == Keys.Space)
                s = " ";

            for (int key = (int)Keys.A; key <= (int)Keys.Z; key++) // I looked up the enum, Keys.A - Keys.Z are continuous
            {
                if (k.HasFlag((Keys)key))
                {
                    s = k.ToString();
                    if (!shiftHeld)
                        s = s.ToLowerInvariant();
                }
            }

            // TODO: make this actually work, its temporary
            currentMessage = currentMessage.Insert(currentPosition, s);
            currentPosition += s.Length;
        }

        public void ReceiveMessage(string msg, string owner)
        {
            ChatMessage message = new ChatMessage();
            message.MessageColor = Color.DarkRed;
            message.Message = msg;
            message.Owner = owner;
            message.Time = DateTime.Now;
            ChatLog.Add(message.Time.Ticks, message);
        }
    }
}
