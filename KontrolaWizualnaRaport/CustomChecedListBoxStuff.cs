using KontrolaWizualnaRaport.CentalDataStorage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static KontrolaWizualnaRaport.Form1;

namespace KontrolaWizualnaRaport
{
    public class CustomChecedListBoxStuff
    {
        public class CustomCheckedListBox : CheckedListBox
        {
            protected override void OnDrawItem(DrawItemEventArgs e)
            {
                base.OnDrawItem(e);

                if (Items.Count <= 0) return;
                string name = Convert.ToString(Items[e.Index]);
                Color rowColor = Color.White;
                GlobalParameters.smtLinesColors.TryGetValue(name, out rowColor);
                SolidBrush rowBrush = new SolidBrush(rowColor);

                var contentRect = e.Bounds;
                contentRect.X = 16;
                e.Graphics.FillRectangle(rowBrush, contentRect);
                e.Graphics.DrawString(Convert.ToString(Items[e.Index]), e.Font, Brushes.White, contentRect);
            }
            private List<string> _selectedLines = new List<string>();
            public List<string> selectedLines
            {
                get
                {
                    if (_selectedLines.Count == 0) return this.CheckedItems.OfType<object>().Select(li => li.ToString()).ToList();
                    return _selectedLines;
                }
                set
                {
                    _selectedLines = value;
                }
            }
        }

        public static void AddSmtLines(CustomCheckedListBox cB)
        {
            foreach (var smtLine in GlobalParameters.allLinesByHand)
            {
                cB.Items.Add(smtLine, true);
            }
        }

        public static void SetUpListBox(TabPage parentPage, CustomCheckedListBox cB, ActionOnCheck actionOnCheck)
        {
            cB.MouseEnter += cB_MouseEnter;
            cB.MouseLeave += cB_MouseLeave;
            cB.Size = new Size(120, 20);
            cB.Parent = parentPage;
            cB.BringToFront();
            cB.Location = new Point(965, 13);
            cB.ForeColor = Color.Black;
            cB.CheckOnClick = true;
            AddSmtLines(cB);
            cB.ItemCheck += new ItemCheckEventHandler((sender, e) => CB_ItemCheck(sender, e, actionOnCheck));

        }

        private static void CB_ItemCheck(object sender, ItemCheckEventArgs e, ActionOnCheck action)
        {
            CustomCheckedListBox chB = (CustomCheckedListBox)sender;
            List<string> selectedLines = new List<string>();
            foreach (var item in chB.CheckedItems)
                selectedLines.Add(item.ToString());

            if (e.NewValue == CheckState.Checked)
                selectedLines.Add(chB.Items[e.Index].ToString());
            else
                selectedLines.Remove(chB.Items[e.Index].ToString());
            chB.selectedLines = selectedLines;

            action();
        }

        private static void cB_MouseEnter(object sender, EventArgs e)
        {
            CustomCheckedListBox chB = (CustomCheckedListBox)sender;
            chB.Height = 120;
        }

        private static void cB_MouseLeave(object sender, EventArgs e)
        {
            CustomCheckedListBox chB = (CustomCheckedListBox)sender;
            chB.Height = 20;
        }
    }
}
