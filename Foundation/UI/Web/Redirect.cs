/* *********************************************************************
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0.
 * 
 * If a copy of the MPL was not distributed with this file, You can obtain
 * one at http://mozilla.org/MPL/2.0/.
 * 
 * This Source Code Form is “Incompatible With Secondary Licenses”, as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

using System;
using System.Web.UI.WebControls;
using FiftyOne.Foundation.Mobile.Configuration;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Web.UI;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Web;

#if VER4 || VER35

using System.Linq;

#endif

namespace FiftyOne.Foundation.UI.Web
{

    /// <summary>
    /// Manages the redirection element of the configuration.
    /// </summary>
    public class Redirect : BaseUserControl
    {
        #region Constants

        internal const string REGEX_NUMERIC = @"^\d{1,}$";
        internal const string REGEX_FILE = @"^$|^(~|\\|\w).+";
        internal const string REGEX_URL = @"^(\w+://|~)[/\w\d\.{}]+$";
        internal const string VALIDATION_GROUP = "Redirect";
        internal const string EXPRESSION_VALUE = "[Expression]";

        #endregion

        #region Static Methods

        /// <summary>
        /// Used to determine if the regex is a valid one.
        /// </summary>
        /// <param name="regex">Regex to be tested.</param>
        /// <returns>True if the expression is valid, otherwise false.</returns>
        internal static bool CheckIsRegExValid(string regex)
        {
            try
            {
                new Regex(regex, RegexOptions.Compiled);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Sets the error CssClass if an error is present.
        /// </summary>
        /// <param name="validator"></param>
        /// <param name="control"></param>
        /// <param name="className"></param>
        internal static void SetError(BaseValidator validator, WebControl control, string className)
        {
            if (validator.IsValid == false)
            {
                AddCssClass(control, className);
            }
        }

        /// <summary>
        /// Adds a new CssClass to any existing ones already present.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="className"></param>
        private static void AddCssClass(WebControl control, string className)
        {
            var list = new List<string>(control.CssClass.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            list.Add(className);
            control.CssClass = String.Join(" ", list.ToArray());
        }

        #endregion

        #region Child Classes

        /// <summary>
        /// Control used to represent a filter assigned to a location. All filters need to 
        /// match for the location to be used.
        /// </summary>
        internal class FilterControl : TableRow
        {
            #region Fields

            private DropDownList _ddlProperties = null;
            private DropDownList _ddlValues = null;
            private TextBox _textBoxExpression = null;
            private CheckBox _checkBoxEnabled = null;
            private Button _buttonDelete = null;
            private CustomValidator _customValidatorMatchExpression = null;
            
            private FilterData _data;
            private Redirect _parent;

            #endregion

            #region Properties

            private int TableIndex
            {
                get { return ((Table)((TableRow)((TableCell)((Table)Parent).Parent).Parent).Parent).Controls.IndexOf(Parent.Parent.Parent); }
            }

            private int RowIndex
            {
                get { return ((Table)Parent).Controls.IndexOf(this); }
            }

            #endregion

            #region Constrcutor

            internal FilterControl(FilterData data, Redirect parent)
            {
                _data = data;
                _parent = parent;

                _ddlProperties = new DropDownList();
                _ddlValues = new DropDownList();
                _textBoxExpression = new TextBox();
                _checkBoxEnabled = new CheckBox();
                _buttonDelete = new Button();
                _customValidatorMatchExpression = new CustomValidator();

                var list = new List<string>();
                foreach (var property in DataProvider.Provider.Properties)
                    list.Add(property.Name);
                list.Sort();
                _ddlProperties.DataSource = list;

                _ddlProperties.AutoPostBack = true;
                _ddlValues.AutoPostBack = true;

                _customValidatorMatchExpression.Display = ValidatorDisplay.None;
                _customValidatorMatchExpression.ValidateEmptyText = true;
                _customValidatorMatchExpression.ValidationGroup = VALIDATION_GROUP;

                // Bind the list of properties to the control.
                _ddlProperties.DataBind();
                _ddlProperties.Items.Add(new ListItem(
                    FiftyOne.Foundation.Mobile.Redirection.Constants.ORIGINAL_URL_SPECIAL_PROPERTY,
                    FiftyOne.Foundation.Mobile.Redirection.Constants.ORIGINAL_URL_SPECIAL_PROPERTY));
                _ddlProperties.SelectedValue =
                    FiftyOne.Foundation.Mobile.Redirection.Constants.ORIGINAL_URL_SPECIAL_PROPERTY;

                SetValuesDataSource();

                _textBoxExpression.Text = _data.MatchExpression;
                _checkBoxEnabled.Checked = _data.Enabled;
            }

            #endregion

            #region Methods

            private TableCell NewCell(Control control)
            {
                var cell = new TableCell();
                cell.Controls.Add(control);
                return cell;
            }

            private void SetValuesDataSource()
            {
                // Get the currently selected property.
#if VER4 || VER35
                var property = DataProvider.Provider.Properties.FirstOrDefault(i =>
                    i.Name == _data.Property);
#else
                FiftyOne.Foundation.Mobile.Detection.Property property = null;
                foreach (var item in DataProvider.Provider.Properties)
                {
                    if (item.Name == _data.Property)
                    {
                        property = item;
                        break;
                    }
                }
#endif

                if (property != null &&
                    property.Values != null &&
                    property.Values.Count > 0)
                {
                    // Select the correct value for the property.
                    _ddlProperties.SelectedValue = property.Name;

                    // Bind the values list to the ones that are available
                    // for the property selected.
                    _ddlValues.Items.Clear();
                    var list = new List<string>();
                    foreach (var value in property.Values)
                        list.Add(value.Name);
                    list.Sort();
                    foreach (var item in list)
                        _ddlValues.Items.Add(new ListItem(item, item));
                    _ddlValues.Items.Add(new ListItem(EXPRESSION_VALUE, String.Empty));

                    // Selected the current item if available, otherwise select
                    // the 1st item in the list.
                    if (String.IsNullOrEmpty(_data.MatchExpression) == false)
                    {
#if VER4 || VER35
                        var selectedValue = property.Values.FirstOrDefault(i =>
                            i.Name == _data.MatchExpression);
#else
                        FiftyOne.Foundation.Mobile.Detection.Value selectedValue = null;
                        foreach (var item in property.Values)
                        {
                            if (item.Name == _data.MatchExpression)
                            {
                                selectedValue = item;
                                break;
                            }
                        }
#endif
                        if (selectedValue != null)
                            _ddlValues.SelectedValue = selectedValue.Name;
                    }
   
                    _data.MatchExpression = _ddlValues.SelectedValue ?? _ddlValues.Items[0].Value;
                    _textBoxExpression.Text = _data.MatchExpression;

                    _ddlValues.Enabled = true;
                }
                else
                {
                    _ddlValues.Enabled = false;
                }
                _textBoxExpression.Enabled = 
                    _ddlValues.Enabled == false ||
                    _ddlValues.SelectedValue == String.Empty;
            }

            #endregion

            #region Events

            protected override void OnInit(EventArgs e)
            {
                base.OnInit(e);

                Cells.Add(NewCell(_ddlProperties));
                Cells.Add(NewCell(_ddlValues));
                Cells.Add(NewCell(_textBoxExpression));
                Cells.Add(NewCell(_checkBoxEnabled));

                var cellLast = new TableCell();
                cellLast.Controls.Add(_buttonDelete);
                cellLast.Controls.Add(_customValidatorMatchExpression);
                Cells.Add(cellLast);

                _textBoxExpression.ID = _customValidatorMatchExpression.ControlToValidate = String.Format("X{0}-{1}", TableIndex, RowIndex);
                _ddlProperties.ID = String.Format("P{0}-{1}", TableIndex, RowIndex);
                _ddlValues.ID = String.Format("V{0}-{1}", TableIndex, RowIndex);
                _buttonDelete.ID = String.Format("D{0}-{1}", TableIndex, RowIndex);
                _checkBoxEnabled.ID = String.Format("E{0}-{1}", TableIndex, RowIndex);

                _ddlProperties.SelectedIndexChanged += new EventHandler(_ddlProperties_SelectedIndexChanged);
                _ddlValues.SelectedIndexChanged += new EventHandler(_ddlValues_SelectedIndexChanged);
                _textBoxExpression.TextChanged += new EventHandler(_textBoxExpression_TextChanged);
                _checkBoxEnabled.CheckedChanged += new EventHandler(_checkBoxEnabled_CheckedChanged);
                _buttonDelete.Click += new EventHandler(_buttonDelete_Click);
                _customValidatorMatchExpression.ServerValidate += new ServerValidateEventHandler(_customValidatorMatchExpression_ServerValidate);
            }

            protected override void OnPreRender(EventArgs e)
            {
                base.OnPreRender(e);

                _checkBoxEnabled.CssClass = _parent.CheckBoxCssClass;
                _textBoxExpression.CssClass = _parent.TextBoxCssClass;
                _ddlValues.CssClass = _parent.DropDownListCssClass;
                _ddlProperties.CssClass = _parent.DropDownListCssClass;
                _buttonDelete.CssClass = _parent.ButtonCssClass;

                _buttonDelete.Text = _parent.RedirectButtonRemoveText;

                _customValidatorMatchExpression.ErrorMessage = _parent.RedirectErrorMessageMatchExpressionText;
                
                SetError(_customValidatorMatchExpression, _textBoxExpression,_parent.ErrorCssClass);
            }

            private void _checkBoxEnabled_CheckedChanged(object sender, EventArgs e)
            {
                _data.Enabled = ((CheckBox)sender).Checked;
            }
            
            private void _customValidatorMatchExpression_ServerValidate(object source, ServerValidateEventArgs args)
            {
                args.IsValid = CheckIsRegExValid(args.Value);
            }

            private void _buttonDelete_Click(object sender, EventArgs e)
            {
                _data.Parent.Remove(_data);
                this.Visible = false;
            }

            private void _textBoxExpression_TextChanged(object sender, EventArgs e)
            {
                _data.MatchExpression = ((TextBox)sender).Text;
                _ddlValues.SelectedValue = 
                    _ddlValues.Items.FindByValue(_data.MatchExpression) != null ? 
                    _data.MatchExpression : String.Empty;
            }

            private void _ddlValues_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (String.IsNullOrEmpty(((DropDownList)sender).SelectedValue) == false)
                {
                    _data.MatchExpression = ((DropDownList)sender).SelectedValue;
                    _textBoxExpression.Text = _data.MatchExpression;
                    _textBoxExpression.Enabled = false;
                }
                else
                    _textBoxExpression.Enabled = true;
            }

            private void _ddlProperties_SelectedIndexChanged(object sender, EventArgs e)
            {
                _data.Property = ((DropDownList)sender).SelectedValue;
                SetValuesDataSource();
            }

            #endregion
        }

        /// <summary>
        /// Class represents a location under the redirect control.
        /// </summary>
        internal class LocationControl : TableRow
        {
            #region Fields

            private Table _tableFilters = null;
            private Panel _panelProperty = null;
            private Panel _panelValue = null;
            private Panel _panelMatchExpression = null;
            private Panel _panelEnabled = null;
            private TextBox _textBoxUrl = null;
            private TextBox _textBoxMatchExpression = null;
            private TextBox _textBoxName = null;
            private Button _buttonAdd = null;
            private Button _buttonRemove = null;
            private Button _buttonToggle = null;
            private RegularExpressionValidator _regularExpressionValidatorUrl;
            private RequiredFieldValidator _requiredFieldValidatorName;
            private CustomValidator _customValidatorMatchExpression;

            private LocationData _data = null;
            private Redirect _parent;

            #endregion

            #region Properties

            internal TextBox TextBoxName
            {
                get { return _textBoxName; }
            }

            internal Table TableFilters
            {
                get { return _tableFilters; }
            }

            private int RowIndex
            {
                get { return ((Table)Parent).Controls.IndexOf(this); }
            }

            #endregion

            #region Constructors

            internal LocationControl(LocationData data, Redirect parent)
            {
                _data = data;
                _parent = parent;

                _tableFilters = new Table();
                _panelEnabled = new Panel();
                _panelMatchExpression = new Panel();
                _panelProperty = new Panel();
                _panelValue = new Panel();
                _textBoxName = new TextBox();
                _textBoxUrl = new TextBox();
                _textBoxMatchExpression = new TextBox();
                _buttonAdd = new Button();
                _buttonRemove = new Button();
                _buttonToggle = new Button();
                _customValidatorMatchExpression = new CustomValidator();
                _requiredFieldValidatorName = new RequiredFieldValidator();
                _regularExpressionValidatorUrl = new RegularExpressionValidator();

                _textBoxName.Text = _data.Name;
                _textBoxUrl.Text = _data.Url;
                _textBoxMatchExpression.Text = _data.MatchExpression;

                _requiredFieldValidatorName.EnableClientScript =
                    _regularExpressionValidatorUrl.EnableClientScript = false;

                _requiredFieldValidatorName.ValidationGroup =
                    _customValidatorMatchExpression.ValidationGroup =
                    _regularExpressionValidatorUrl.ValidationGroup = VALIDATION_GROUP;

                _requiredFieldValidatorName.Display =
                    _customValidatorMatchExpression.Display =
                    _regularExpressionValidatorUrl.Display = ValidatorDisplay.None;

                _regularExpressionValidatorUrl.ValidationExpression = REGEX_URL;

                var rowHeader = new TableHeaderRow();
                rowHeader.Cells.Add(NewCell(_panelProperty));
                rowHeader.Cells.Add(NewCell(_panelValue));
                rowHeader.Cells.Add(NewCell(_panelMatchExpression));
                rowHeader.Cells.Add(NewCell(_panelEnabled));
                _tableFilters.Rows.Add(rowHeader);

                foreach (FilterData filter in _data)
                    _tableFilters.Rows.Add(new FilterControl(filter, _parent));
            }

            #endregion

            #region Methods

            private TableCell NewCell(Control control)
            {
                var cell = new TableCell();
                cell.Controls.Add(control);
                return cell;
            }

            #endregion

            #region Events

            protected override void OnInit(EventArgs e)
            {
                base.OnInit(e);

                Cells.Add(NewCell(_textBoxName));
                Cells.Add(NewCell(_textBoxUrl));
                Cells.Add(NewCell(_textBoxMatchExpression));
                Cells.Add(NewCell(_buttonAdd));
                Cells.Add(NewCell(_buttonToggle));

                var cellLast =  new TableCell();
                cellLast.Controls.Add(_buttonRemove);
                cellLast.Controls.Add(_customValidatorMatchExpression);
                cellLast.Controls.Add(_regularExpressionValidatorUrl);
                cellLast.Controls.Add(_requiredFieldValidatorName);
                Cells.Add(cellLast);

                _textBoxName.ID = _requiredFieldValidatorName.ControlToValidate = String.Format("N{0}", RowIndex);
                _textBoxUrl.ID = _regularExpressionValidatorUrl.ControlToValidate = String.Format("U{0}", RowIndex);
                _textBoxMatchExpression.ID = _customValidatorMatchExpression.ControlToValidate = String.Format("M{0}", RowIndex);
                _buttonAdd.ID = String.Format("A{0}", RowIndex);
                _buttonRemove.ID = String.Format("R{0}", RowIndex);

                _textBoxName.TextChanged += new EventHandler(_textBoxName_TextChanged);
                _textBoxUrl.TextChanged += new EventHandler(_textBoxUrl_TextChanged);
                _textBoxMatchExpression.TextChanged += new EventHandler(_textBoxMatchExpression_TextChanged);
                _buttonAdd.Click += new EventHandler(_buttonAdd_Click);
                _buttonRemove.Click += new EventHandler(_buttonRemove_Click);
                _buttonToggle.Click += new EventHandler(_buttonToggle_Click);
                _customValidatorMatchExpression.ServerValidate += new ServerValidateEventHandler(_customValidatorMatchExpression_ServerValidate);
            }

            void _buttonToggle_Click(object sender, EventArgs e)
            {
                _data.ShowFilters = !_data.ShowFilters;
            }

            void _buttonRemove_Click(object sender, EventArgs e)
            {
                _data.Parent.Remove(_data);
                if (_tableFilters.Parent.Parent != null)
                    Parent.Controls.Remove(_tableFilters.Parent.Parent);
                Parent.Controls.Remove(this);
            }
            
            protected override void OnPreRender(EventArgs e)
            {
                base.OnPreRender(e);

                _buttonToggle.Visible = false;
                if (_tableFilters.Parent != null &&
                    _tableFilters.Parent.Parent != null)
                {
                    _tableFilters.Parent.Parent.Visible = _data.ShowFilters;
                }

                _buttonToggle.Visible = _tableFilters.Visible = _data.Count > 0;

                _parent.AddLabel(_panelProperty,
                    _parent.RedirectLabelPropertyText,
                    _parent.RedirectLocationFilterPropertyToolTip,
                    null, null);
                _parent.AddLabel(_panelValue,
                    _parent.RedirectLabelValueText,
                    _parent.RedirectLocationFilterMatchExpressionToolTip,
                    null, null);
                _parent.AddLabel(_panelMatchExpression,
                    _parent.RedirectLabelMatchExpressionText,
                    _parent.RedirectLocationFilterMatchExpressionToolTip,
                    null, null);
                _parent.AddLabel(_panelEnabled,
                    _parent.RedirectLabelEnabledText,
                    null,
                    null, null);

                AddCssClass(_panelProperty, _parent.LabelCssClass);
                AddCssClass(_panelValue, _parent.LabelCssClass);
                AddCssClass(_panelMatchExpression, _parent.LabelCssClass);
                AddCssClass(_panelEnabled, _parent.LabelCssClass);
                AddCssClass(_buttonAdd, _parent.ButtonCssClass);
                AddCssClass(_textBoxUrl, _parent.TextBoxCssClass);
                AddCssClass(_textBoxMatchExpression, _parent.TextBoxCssClass);
                AddCssClass(_textBoxName, _parent.TextBoxCssClass);

                _buttonAdd.Text = _parent.RedirectButtonNewText;
                _buttonRemove.Text = _parent.RedirectButtonRemoveText;
                _buttonToggle.Text = _data.ShowFilters ? _parent.RedirectButtonHideText : _parent.RedirectButtonShowText;

                _customValidatorMatchExpression.ErrorMessage = _parent.RedirectErrorMessageMatchExpressionText;
                _requiredFieldValidatorName.ErrorMessage = _parent.RedirectErrorMessageNameFieldText;
                _regularExpressionValidatorUrl.ErrorMessage = _parent.RedirectErrorMessageUrlFormatText;

                SetError(_customValidatorMatchExpression, _textBoxMatchExpression, _parent.ErrorCssClass);
                SetError(_requiredFieldValidatorName, _textBoxName, _parent.ErrorCssClass);
                SetError(_regularExpressionValidatorUrl, _textBoxUrl, _parent.ErrorCssClass);
            }

            private void _customValidatorMatchExpression_ServerValidate(object source, ServerValidateEventArgs args)
            {
                args.IsValid = CheckIsRegExValid(args.Value);
            }

            private void _buttonAdd_Click(object sender, EventArgs e)
            {
                var newFilter = new FilterData(_data);
                _data.Add(newFilter);
                _tableFilters.Rows.Add(new FilterControl(newFilter, _parent));
                _data.ShowFilters = true;
            }

            private void _textBoxMatchExpression_TextChanged(object sender, EventArgs e)
            {
                _data.MatchExpression = ((TextBox)sender).Text;
            }

            private void _textBoxUrl_TextChanged(object sender, EventArgs e)
            {
                _data.Url = ((TextBox)sender).Text;
            }
            
            private void _textBoxName_TextChanged(object sender, EventArgs e)
            {
                _data.Name = ((TextBox)sender).Text;
            }

            #endregion
        }

        /// <summary>
        /// Represents a collection of locations in the control.
        /// </summary>
        internal class LocationsControl : Table
        {
            #region Fields

            private Panel _panelName;
            private Panel _panelUrl;
            private Panel _panelMatchExpression;
            private Panel _panelFilters;
            private CustomValidator _customValidatorUniqueName;
            private RedirectData _data;
            private Redirect _parent;
            private List<LocationControl> _listDuplicateNames = new List<LocationControl>();

            #endregion

            #region Constructor

            internal LocationsControl (RedirectData data, Redirect parent)
            {
                _data = data;
                _parent = parent;
                _panelName = new Panel();
                _panelUrl = new Panel();
                _panelMatchExpression = new Panel();
                _panelFilters = new Panel();
                _customValidatorUniqueName = new CustomValidator();
            }

            #endregion

            #region Methods

            private TableCell NewCell(Control control)
            {
                var cell = new TableCell();
                cell.Controls.Add(control);
                return cell;
            }

            #endregion

            #region Events

            /// <summary>
            /// Initialise the controls.
            /// </summary>
            /// <param name="e"></param>
            protected override void OnInit(EventArgs e)
            {
                base.OnInit(e);

                var rowHeader = new TableHeaderRow();
                rowHeader.Cells.Add(NewCell(_panelName));
                rowHeader.Cells.Add(NewCell(_panelUrl));
                rowHeader.Cells.Add(NewCell(_panelMatchExpression));
                var cellFiltersLabel = new TableCell();
                cellFiltersLabel.ColumnSpan = 2;
                cellFiltersLabel.Controls.Add(_panelFilters);
                cellFiltersLabel.Controls.Add(_customValidatorUniqueName);
                rowHeader.Cells.Add(cellFiltersLabel);
                Rows.Add(rowHeader);

                _customValidatorUniqueName.ControlToValidate = ID;
                _customValidatorUniqueName.Display = ValidatorDisplay.None;
                _customValidatorUniqueName.ValidationGroup = VALIDATION_GROUP;

                foreach (LocationData item in _data)
                {
                    // Add the location row.
                    var locationRow = new LocationControl(item, _parent);

                    // Add the filters associated with the location to the next row.
                    Rows.Add(locationRow);
                    var rowFilters = new TableRow();
                    var cellFilters = new TableCell();
                    rowFilters.Cells.Add(cellFilters);
                    cellFilters.ColumnSpan = 6;
                    Rows.Add(rowFilters);
                    cellFilters.Controls.Add(locationRow.TableFilters);
                }

                _customValidatorUniqueName.ServerValidate += new ServerValidateEventHandler(_customValidatorUniqueName_ServerValidate);
            }

            /// <summary>
            /// Check for duplicate unique names for the location elements. If duplicates are found raise
            /// an error and ensure the list of offending controls is updated to mark in the returned UI.
            /// </summary>
            /// <param name="source"></param>
            /// <param name="args"></param>
            private void _customValidatorUniqueName_ServerValidate(object source, ServerValidateEventArgs args)
            {
                var counts = new SortedList<string, int>();
                foreach (TableRow row in Rows)
                {
                    if (row is LocationControl)
                    {
                        var locationRow = (LocationControl)row;
                        if (counts.ContainsKey(locationRow.TextBoxName.Text) == false)
                            counts.Add(locationRow.TextBoxName.Text, 1);
                        else
                            counts[locationRow.TextBoxName.Text]++;
                    }
                }

                var list = new List<string>();
                foreach (var key in counts.Keys)
                    if (counts[key] > 1)
                        list.Add(key);

                foreach (TableRow row in Rows)
                {
                    if (row is LocationControl)
                    {
                        var locationRow = (LocationControl)row;
                        if (counts[locationRow.TextBoxName.Text] > 1)
                            _listDuplicateNames.Add(locationRow);
                    }
                }

                _customValidatorUniqueName.ErrorMessage = String.Format(
                    _parent.RedirectErrorMessageDuplicatesText,
                    String.Join(", ", list.ToArray()));

                args.IsValid = list.Count == 0;
            }

            /// <summary>
            /// Add any UI information to the controls.
            /// </summary>
            /// <param name="e"></param>
            protected override void OnPreRender(EventArgs e)
            {
                base.OnPreRender(e);

                _panelName.CssClass = _panelUrl.CssClass = _panelMatchExpression.CssClass =
                    _panelFilters.CssClass = _parent.LabelCssClass;

                _parent.AddLabel(_panelName,
                    _parent.RedirectLabelUniqueNameText,
                    _parent.RedirectLocationNameToolTip,
                    null, null);
                _parent.AddLabel(_panelUrl,
                    _parent.RedirectLabelRedirectUrlText,
                    _parent.RedirectLocationUrlToolTip,
                    null, null);
                _parent.AddLabel(_panelMatchExpression,
                    _parent.RedirectLabelMatchExpressionText,
                    _parent.RedirectLocationMatchExpressionToolTip,
                    null, null);
                _parent.AddLabel(_panelFilters,
                    _parent.RedirectLabelFiltersText,
                    _parent.RedirectLocationFiltersToolTip,
                    null, null);

                foreach (var row in _listDuplicateNames)
                    row.TextBoxName.CssClass = _parent.ErrorCssClass;

                int count = 1;
                foreach (TableRow row in Rows)
                {
                    if (row is LocationControl)
                    {
                        row.CssClass = count % 2 == 0 ? _parent.AltLocationCssClass : _parent.LocationCssClass;
                        int filterRowIndex = Rows.GetRowIndex(row) + 1;
                        if (filterRowIndex < Rows.Count)
                            Rows[filterRowIndex].CssClass = row.CssClass;
                        count++;
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Fields

        #region Controls

        private Table _tableBasic = null;
        private Panel _panelEnabled = null;
        private Panel _panelDevicesFile = null;
        private Panel _panelFirstRequestOnly = null;
        private Panel _panelMobileHomePageUrl = null;
        private Panel _panelMobilePagesRegex = null;
        private Panel _panelOriginalUrlAsQueryString = null;
        private Panel _panelTimeout = null;
        private Literal _literalBasic = null;
        private Literal _literalLocations = null;
        private Literal _literalMessage = null;

        private CheckBox _checkBoxEnabled = null;
        private TextBox _textBoxDevicesFile = null;
        private CheckBox _checkBoxFirstRequestOnly = null;
        private TextBox _textBoxMobileHomePageUrl = null;
        private TextBox _textBoxMobilePagesRegex = null;
        private CheckBox _checkBoxOriginalUrlAsQueryString = null;
        private TextBox _textBoxTimeout = null;
        private ValidationSummary _validationSummary = null;
        private RegularExpressionValidator _regularExpressionValidatorTimeout = null;
        private RegularExpressionValidator _regularExpressionValidatorUrl = null;
        private RegularExpressionValidator _regularExpressionValidatorDevicesFile = null;
        private CustomValidator _customValidatorRegex = null;
        private LocationsControl _locationsControl = null;
        private Button _buttonUpdate = null;
        private Button _buttonReset = null;
        private Button _buttonAdd = null;
        private Panel _panelLocations = null;
        private Panel _panelButtons = null;
        private Panel _panelBasic = null;
        private Panel _panelMessages = null;
        private RedirectData _data = null;

        #endregion

        #region Css

        private string _labelCssClass = "label";
        private string _dropDownListCssClass = "ddl";
        private string _textBoxCssClass = "textbox";
        private string _checkBoxCssClass = "checkbox";
        private string _errorCssClass = "error";
        private string _successCssClass = "success";
        private string _buttonCssClass = "button";
        private string _locationCssClass = "location";
        private string _altLocationCssClass = "altlocation";
        private string _basicCssClass = "basic";
        private string _locationsCssClass = "locations";
        private string _buttonsCssClass = "buttons";

        #endregion

        #region Messages

        private string _redirectBasicHeadingHtml = Resources.RedirectBasicHeadingHtml;
        private string _redirectButtonHideText = Resources.RedirectButtonHideText;
        private string _redirectButtonNewLocationText = Resources.RedirectButtonNewLocationText;
        private string _redirectButtonNewText = Resources.RedirectButtonNewText;
        private string _redirectButtonRemoveText = Resources.RedirectButtonRemoveText;
        private string _redirectButtonResetText = Resources.RedirectButtonResetText;
        private string _redirectButtonShowText = Resources.RedirectButtonShowText;
        private string _redirectButtonUpdateText = Resources.RedirectButtonUpdateText;
        private string _redirectDevicesFileToolTip = Resources.RedirectDevicesFileToolTip;
        private string _redirectEnabledToolTip = Resources.RedirectEnabledToolTip;
        private string _redirectErrorMessageDevicesFileText = Resources.RedirectErrorMessageDevicesFileText;
        private string _redirectErrorMessageDuplicatesText = Resources.RedirectErrorMessageDuplicatesText;
        private string _redirectErrorMessageMatchExpressionText = Resources.RedirectErrorMessageMatchExpressionText;
        private string _redirectErrorMessageMobileHomePageRegexText = Resources.RedirectErrorMessageMobileHomePageRegexText;
        private string _redirectErrorMessageMobileHomePageUrlText = Resources.RedirectErrorMessageMobileHomePageUrlText;
        private string _redirectErrorMessageNameFieldText = Resources.RedirectErrorMessageNameFieldText;
        private string _redirectErrorMessageTimeOutText = Resources.RedirectErrorMessageTimeOutText;
        private string _redirectErrorMessageUrlFormatText = Resources.RedirectErrorMessageUrlFormatText;
        private string _redirectFirstRequestOnlyToolTip = Resources.RedirectFirstRequestOnlyToolTip;
        private string _redirectLabelDevicesFileText = Resources.RedirectLabelDevicesFileText;
        private string _redirectLabelEnabledText = Resources.RedirectLabelEnabledText;
        private string _redirectLabelFiltersText = Resources.RedirectLabelFiltersText;
        private string _redirectLabelFirstRequestOnlyText = Resources.RedirectLabelFirstRequestOnlyText;
        private string _redirectLabelMatchExpressionText = Resources.RedirectLabelMatchExpressionText;
        private string _redirectLabelMobileHomePageUrlText = Resources.RedirectLabelMobileHomePageUrlText;
        private string _redirectLabelMobilePagesRegexText = Resources.RedirectLabelMobilePagesRegexText;
        private string _redirectLabelOriginalUrlAsQueryStringText = Resources.RedirectLabelOriginalUrlAsQueryStringText;
        private string _redirectLabelPropertyText = Resources.RedirectLabelPropertyText;
        private string _redirectLabel_redirectUrlText = Resources.RedirectLabelRedirectUrlText;
        private string _redirectLabelTimeoutText = Resources.RedirectLabelTimeoutText;
        private string _redirectLabelUniqueNameText = Resources.RedirectLabelUniqueNameText;
        private string _redirectLabelValueText = Resources.RedirectLabelValueText;
        private string _redirectLocationFilterMatchExpressionToolTip = Resources.RedirectLocationFilterMatchExpressionToolTip;
        private string _redirectLocationFilterPropertyToolTip = Resources.RedirectLocationFilterPropertyToolTip;
        private string _redirectLocationFiltersToolTip = Resources.RedirectLocationFiltersToolTip;
        private string _redirectLocationMatchExpressionToolTip = Resources.RedirectLocationMatchExpressionToolTip;
        private string _redirectLocationNameToolTip = Resources.RedirectLocationNameToolTip;
        private string _redirectLocationsHeadingHtml = Resources.RedirectLocationsHeadingHtml;
        private string _redirectLocationUrlToolTip = Resources.RedirectLocationUrlToolTip;
        private string _redirectMobilePagesRegexToolTip = Resources.RedirectMobilePagesRegexToolTip;
        private string _redirectMobile_redirectUrlToolTip = Resources.RedirectMobileRedirectUrlToolTip;
        private string _redirectOriginalUrlAsQueryStringToolTip = Resources.RedirectOriginalUrlAsQueryStringToolTip;
        private string _redirectTimeoutToolTip = Resources.RedirectTimeoutToolTip;
        private string _redirectUpdateGeneralDetailedFailureHtml = Resources.RedirectUpdateGeneralDetailedFailureHtml;
        private string _redirectUpdateGeneralFailureHtml = Resources.RedirectUpdateGeneralFailureHtml;
        private string _redirectUpdateSuccessHtml = Resources.RedirectUpdateSuccessHtml;

        #endregion

        #endregion

        #region Properties

        #region Css

        /// <summary>
        /// The CssClass used for the general buttons area of the control.
        /// </summary>
        public string CssClassButtons
        {
            get { return _buttonsCssClass; }
            set { _buttonsCssClass = value; }
        }

        /// <summary>
        /// The CssClass used for the basic fields areas of the the control.
        /// </summary>
        public string CssClassBasic
        {
            get { return _basicCssClass; }
            set { _basicCssClass = value; }
        }

        /// <summary>
        /// The CssClass used for the locations area of the control.
        /// </summary>
        public string CssClassLocations
        {
            get { return _locationsCssClass; }
            set { _locationsCssClass = value; }
        }

        /// <summary>
        /// The CssClass used for every other location row in the table.
        /// </summary>
        public string LocationCssClass
        {
            get { return _locationCssClass; }
            set { _locationCssClass = value; }
        }

        /// <summary>
        /// The CssClass used for every other location row in the table.
        /// </summary>
        public string AltLocationCssClass
        {
            get { return _altLocationCssClass; }
            set { _altLocationCssClass = value; }
        }

        /// <summary>
        /// The general button class used by the control.
        /// </summary>
        public string ButtonCssClass
        {
            get { return _buttonCssClass; }
            set { _buttonCssClass = value; }
        }

        /// <summary>
        /// The CssClass used when an error is displayed.
        /// </summary>
        public string ErrorCssClass
        {
            get { return _errorCssClass; }
            set { _errorCssClass = value; }
        }

        /// <summary>
        /// The CssClass used when a success message is displayed.
        /// </summary>
        public string SuccessCssClass
        {
            get { return _successCssClass; }
            set { _successCssClass = value; }
        }

        /// <summary>
        /// The CssClass used to display a label.
        /// </summary>
        public string LabelCssClass
        {
            get { return _labelCssClass; }
            set { _labelCssClass = value; }
        }

        /// <summary>
        /// The CssClass used to display a drop down list.
        /// </summary>
        public string DropDownListCssClass
        {
            get { return _dropDownListCssClass; }
            set { _dropDownListCssClass = value; }
        }

        /// <summary>
        /// The CssClass used to display a textbox.
        /// </summary>
        public string TextBoxCssClass
        {
            get { return _textBoxCssClass; }
            set { _textBoxCssClass = value; }
        }

        /// <summary>
        /// The CssClass used to display a checkbox.
        /// </summary>
        public string CheckBoxCssClass
        {
            get { return _checkBoxCssClass; }
            set { _checkBoxCssClass = value; }
        }

        #endregion

        #region Messages

        // Properties used to localise text displayed by the control.

        #pragma warning disable 1591

        public string RedirectBasicHeadingHtml { get { return _redirectBasicHeadingHtml; } set { _redirectBasicHeadingHtml = value; } }
        public string RedirectButtonHideText { get { return _redirectButtonHideText; } set { _redirectButtonHideText = value; } }
        public string RedirectButtonNewLocationText { get { return _redirectButtonNewLocationText; } set { _redirectButtonNewLocationText = value; } }
        public string RedirectButtonNewText { get { return _redirectButtonNewText; } set { _redirectButtonNewText = value; } }
        public string RedirectButtonRemoveText { get { return _redirectButtonRemoveText; } set { _redirectButtonRemoveText = value; } }
        public string RedirectButtonResetText { get { return _redirectButtonResetText; } set { _redirectButtonResetText = value; } }
        public string RedirectButtonShowText { get { return _redirectButtonShowText; } set { _redirectButtonShowText = value; } }
        public string RedirectButtonUpdateText { get { return _redirectButtonUpdateText; } set { _redirectButtonUpdateText = value; } }
        public string RedirectDevicesFileToolTip { get { return _redirectDevicesFileToolTip; } set { _redirectDevicesFileToolTip = value; } }
        public string RedirectEnabledToolTip { get { return _redirectEnabledToolTip; } set { _redirectEnabledToolTip = value; } }
        public string RedirectErrorMessageDevicesFileText { get { return _redirectErrorMessageDevicesFileText; } set { _redirectErrorMessageDevicesFileText = value; } }
        public string RedirectErrorMessageDuplicatesText { get { return _redirectErrorMessageDuplicatesText; } set { _redirectErrorMessageDuplicatesText = value; } }
        public string RedirectErrorMessageMatchExpressionText { get { return _redirectErrorMessageMatchExpressionText; } set { _redirectErrorMessageMatchExpressionText = value; } }
        public string RedirectErrorMessageMobileHomePageRegexText { get { return _redirectErrorMessageMobileHomePageRegexText; } set { _redirectErrorMessageMobileHomePageRegexText = value; } }
        public string RedirectErrorMessageMobileHomePageUrlText { get { return _redirectErrorMessageMobileHomePageUrlText; } set { _redirectErrorMessageMobileHomePageUrlText = value; } }
        public string RedirectErrorMessageNameFieldText { get { return _redirectErrorMessageNameFieldText; } set { _redirectErrorMessageNameFieldText = value; } }
        public string RedirectErrorMessageTimeOutText { get { return _redirectErrorMessageTimeOutText; } set { _redirectErrorMessageTimeOutText = value; } }
        public string RedirectErrorMessageUrlFormatText { get { return _redirectErrorMessageUrlFormatText; } set { _redirectErrorMessageUrlFormatText = value; } }
        public string RedirectFirstRequestOnlyToolTip { get { return _redirectFirstRequestOnlyToolTip; } set { _redirectFirstRequestOnlyToolTip = value; } }
        public string RedirectLabelDevicesFileText { get { return _redirectLabelDevicesFileText; } set { _redirectLabelDevicesFileText = value; } }
        public string RedirectLabelEnabledText { get { return _redirectLabelEnabledText; } set { _redirectLabelEnabledText = value; } }
        public string RedirectLabelFiltersText { get { return _redirectLabelFiltersText; } set { _redirectLabelFiltersText = value; } }
        public string RedirectLabelFirstRequestOnlyText { get { return _redirectLabelFirstRequestOnlyText; } set { _redirectLabelFirstRequestOnlyText = value; } }
        public string RedirectLabelMatchExpressionText { get { return _redirectLabelMatchExpressionText; } set { _redirectLabelMatchExpressionText = value; } }
        public string RedirectLabelMobileHomePageUrlText { get { return _redirectLabelMobileHomePageUrlText; } set { _redirectLabelMobileHomePageUrlText = value; } }
        public string RedirectLabelMobilePagesRegexText { get { return _redirectLabelMobilePagesRegexText; } set { _redirectLabelMobilePagesRegexText = value; } }
        public string RedirectLabelOriginalUrlAsQueryStringText { get { return _redirectLabelOriginalUrlAsQueryStringText; } set { _redirectLabelOriginalUrlAsQueryStringText = value; } }
        public string RedirectLabelPropertyText { get { return _redirectLabelPropertyText; } set { _redirectLabelPropertyText = value; } }
        public string RedirectLabelRedirectUrlText { get { return _redirectLabel_redirectUrlText; } set { _redirectLabel_redirectUrlText = value; } }
        public string RedirectLabelTimeoutText { get { return _redirectLabelTimeoutText; } set { _redirectLabelTimeoutText = value; } }
        public string RedirectLabelUniqueNameText { get { return _redirectLabelUniqueNameText; } set { _redirectLabelUniqueNameText = value; } }
        public string RedirectLabelValueText { get { return _redirectLabelValueText; } set { _redirectLabelValueText = value; } }
        public string RedirectLocationFilterMatchExpressionToolTip { get { return _redirectLocationFilterMatchExpressionToolTip; } set { _redirectLocationFilterMatchExpressionToolTip = value; } }
        public string RedirectLocationFilterPropertyToolTip { get { return _redirectLocationFilterPropertyToolTip; } set { _redirectLocationFilterPropertyToolTip = value; } }
        public string RedirectLocationFiltersToolTip { get { return _redirectLocationFiltersToolTip; } set { _redirectLocationFiltersToolTip = value; } }
        public string RedirectLocationMatchExpressionToolTip { get { return _redirectLocationMatchExpressionToolTip; } set { _redirectLocationMatchExpressionToolTip = value; } }
        public string RedirectLocationNameToolTip { get { return _redirectLocationNameToolTip; } set { _redirectLocationNameToolTip = value; } }
        public string RedirectLocationsHeadingHtml { get { return _redirectLocationsHeadingHtml; } set { _redirectLocationsHeadingHtml = value; } }
        public string RedirectLocationUrlToolTip { get { return _redirectLocationUrlToolTip; } set { _redirectLocationUrlToolTip = value; } }
        public string RedirectMobilePagesRegexToolTip { get { return _redirectMobilePagesRegexToolTip; } set { _redirectMobilePagesRegexToolTip = value; } }
        public string RedirectMobileRedirectUrlToolTip { get { return _redirectMobile_redirectUrlToolTip; } set { _redirectMobile_redirectUrlToolTip = value; } }
        public string RedirectOriginalUrlAsQueryStringToolTip { get { return _redirectOriginalUrlAsQueryStringToolTip; } set { _redirectOriginalUrlAsQueryStringToolTip = value; } }
        public string RedirectTimeoutToolTip { get { return _redirectTimeoutToolTip; } set { _redirectTimeoutToolTip = value; } }
        public string RedirectUpdateGeneralDetailedFailureHtml { get { return _redirectUpdateGeneralDetailedFailureHtml; } set { _redirectUpdateGeneralDetailedFailureHtml = value; } }
        public string RedirectUpdateGeneralFailureHtml { get { return _redirectUpdateGeneralFailureHtml; } set { _redirectUpdateGeneralFailureHtml = value; } }
        public string RedirectUpdateSuccessHtml { get { return _redirectUpdateSuccessHtml; } set { _redirectUpdateSuccessHtml = value; } }

        #pragma warning restore 1591

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of the <see cref="Redirect"/> control. 
        /// </summary>
        public Redirect()
        {
            CssClass = "redirect";

            _tableBasic = new Table();
            _panelEnabled = new Panel();
            _panelDevicesFile = new Panel();
            _panelFirstRequestOnly = new Panel();
            _panelMobileHomePageUrl = new Panel();
            _panelMobilePagesRegex = new Panel();
            _panelOriginalUrlAsQueryString = new Panel();
            _panelTimeout = new Panel();
            _literalBasic = new Literal();
            _literalLocations = new Literal();
            _textBoxDevicesFile = new TextBox();
            _checkBoxEnabled = new CheckBox();
            _checkBoxFirstRequestOnly = new CheckBox();
            _textBoxMobileHomePageUrl = new TextBox();
            _textBoxMobilePagesRegex = new TextBox();
            _checkBoxOriginalUrlAsQueryString = new CheckBox();
            _textBoxTimeout = new TextBox();
            _buttonReset = new Button();
            _buttonUpdate = new Button();
            _buttonAdd = new Button();
            _validationSummary = new ValidationSummary();
            _customValidatorRegex = new CustomValidator();
            _regularExpressionValidatorDevicesFile = new RegularExpressionValidator();
            _regularExpressionValidatorTimeout = new RegularExpressionValidator();
            _regularExpressionValidatorUrl = new RegularExpressionValidator();
            _panelLocations = new Panel();
            _panelButtons = new Panel();
            _panelBasic = new Panel();
            _panelMessages = new Panel();
            _literalMessage = new Literal();

            _textBoxDevicesFile.ID =
                _regularExpressionValidatorDevicesFile.ControlToValidate = "DevicesFile";
            _textBoxMobileHomePageUrl.ID =
                _regularExpressionValidatorUrl.ControlToValidate = "MobileHomePageUrl";
            _textBoxMobilePagesRegex.ID =
                _customValidatorRegex.ControlToValidate = "MobilePagesRegex";

            _checkBoxFirstRequestOnly.ID = "FirstRequestOnly";
            _checkBoxOriginalUrlAsQueryString.ID = "OriginalUrlAsQueryString";
            _textBoxTimeout.ID = 
                _regularExpressionValidatorTimeout.ControlToValidate = "Timeout";
            _checkBoxEnabled.ID = "Enabled";

            _buttonReset.ID = "Reset";
            _buttonUpdate.ID = "Update";
            _buttonAdd.ID = "Add";

            _buttonUpdate.CausesValidation = true;
            _buttonReset.CausesValidation = _buttonAdd.CausesValidation = false;

            _validationSummary.DisplayMode = ValidationSummaryDisplayMode.List;

            _validationSummary.ValidationGroup =
                _buttonUpdate.ValidationGroup =
                _regularExpressionValidatorDevicesFile.ValidationGroup =
                _regularExpressionValidatorTimeout.ValidationGroup =
                _regularExpressionValidatorUrl.ValidationGroup =
                _customValidatorRegex.ValidationGroup = VALIDATION_GROUP;

            _regularExpressionValidatorDevicesFile.Display =
                _regularExpressionValidatorTimeout.Display =
                _regularExpressionValidatorUrl.Display =
                _customValidatorRegex.Display = ValidatorDisplay.None;

            _regularExpressionValidatorDevicesFile.EnableClientScript =
                _regularExpressionValidatorTimeout.EnableClientScript =
                _regularExpressionValidatorUrl.EnableClientScript = false;

            _regularExpressionValidatorTimeout.ValidationExpression = REGEX_NUMERIC;
            _regularExpressionValidatorDevicesFile.ValidationExpression = REGEX_FILE;
            _regularExpressionValidatorUrl.ValidationExpression = REGEX_URL;

            _buttonUpdate.CausesValidation = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Saves the controls data between requests.
        /// </summary>
        /// <returns>The data object to be saved.</returns>
        protected override object SaveControlState()
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    _data.Serialize(writer);
                }
                return ms.GetBuffer();
            }
        }

        /// <summary>
        /// Loads the controls data from a previous request.
        /// </summary>
        /// <param name="savedState">The previous data object.</param>
        protected override void LoadControlState(object savedState)
        {
            using (var reader = new BinaryReader(new MemoryStream((byte[])savedState)))
            {
                _data = new RedirectData(reader);
                LoadData();
            }
        }

        /// <summary>
        /// Sets the fields based on the data strucuture loaded.
        /// </summary>
        protected void LoadData()
        {
            _textBoxDevicesFile.Text = _data.DevicesFile;
            _checkBoxEnabled.Checked = _data.Enabled;
            _checkBoxFirstRequestOnly.Checked = _data.FirstRequestOnly;
            _textBoxMobileHomePageUrl.Text = _data.MobileHomePageUrl;
            _textBoxMobilePagesRegex.Text = _data.MobilePagesRegex;
            _checkBoxOriginalUrlAsQueryString.Checked = _data.OriginalUrlAsQueryString;
            _textBoxTimeout.Text = _data.Timeout.ToString();

            if (_locationsControl != null)
                _panelLocations.Controls.Remove(_locationsControl);
            _locationsControl = new LocationsControl(_data, this);
            _panelLocations.Controls.Add(_locationsControl);
        }

        private Panel AddPanel()
        {
            var panel = new Panel();
            Controls.Add(panel);
            return panel;
        }

        private TableRow AddTableRow(Panel label, Control ctrl)
        {
            var row = new TableRow();
            _tableBasic.Controls.Add(row);
            var cell1 = new TableCell();
            var cell2 = new TableCell();
            row.Controls.Add(cell1);
            row.Controls.Add(cell2);
            cell1.Controls.Add(label);
            cell2.Controls.Add(ctrl);
            return row;
        }

        private TableRow AddTableRowSingle(Panel label, Control ctrl)
        {
            var row1 = new TableRow();
            var row2 = new TableRow();
            _tableBasic.Controls.Add(row1);
            _tableBasic.Controls.Add(row2);
            var cell1 = new TableCell();
            var cell2 = new TableCell();
            cell1.ColumnSpan = cell2.ColumnSpan = 2;
            row1.Controls.Add(cell1);
            row2.Controls.Add(cell2);
            cell1.Controls.Add(label);
            cell2.Controls.Add(ctrl);
            return row1;
        }

        #endregion

        #region Events

        /// <summary>
        /// Initialise the controls.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Page.MaintainScrollPositionOnPostBack = true;

            _container.Controls.Add(_panelMessages);
            _panelMessages.Controls.Add(_literalMessage);
            _panelMessages.Controls.Add(_validationSummary);
            _container.Controls.Add(_panelBasic);
            _panelBasic.Controls.Add(_literalBasic);
            _panelBasic.Controls.Add(_tableBasic);
            AddTableRow(_panelEnabled, _checkBoxEnabled);
            AddTableRow(_panelFirstRequestOnly, _checkBoxFirstRequestOnly);
            AddTableRow(_panelOriginalUrlAsQueryString, _checkBoxOriginalUrlAsQueryString);
            AddTableRow(_panelTimeout, _textBoxTimeout);
            AddTableRowSingle(_panelMobileHomePageUrl, _textBoxMobileHomePageUrl);
            AddTableRowSingle(_panelMobilePagesRegex, _textBoxMobilePagesRegex);
            AddTableRowSingle(_panelDevicesFile, _textBoxDevicesFile);
            _container.Controls.Add(_panelLocations);
            _panelLocations.Controls.Add(_literalLocations);
            _container.Controls.Add(_panelButtons);
            _panelButtons.Controls.Add(_buttonUpdate);
            _panelButtons.Controls.Add(_buttonReset);
            _panelButtons.Controls.Add(_buttonAdd);
            _container.Controls.Add(_regularExpressionValidatorDevicesFile);
            _container.Controls.Add(_regularExpressionValidatorTimeout);
            _container.Controls.Add(_regularExpressionValidatorUrl);
            _container.Controls.Add(_customValidatorRegex);
                        
            Page.RegisterRequiresControlState(this);

            if (!Page.IsPostBack)
            {
                _data = new RedirectData(FiftyOne.Foundation.Mobile.Configuration.Manager.Redirect);
                LoadData();
            }
            
            _textBoxDevicesFile.TextChanged += new EventHandler(_textBoxDevicesFile_TextChanged);
            _checkBoxEnabled.CheckedChanged += new EventHandler(_checkBoxEnabled_CheckedChanged);
            _checkBoxFirstRequestOnly.CheckedChanged += new EventHandler(_checkBoxFirstRequestOnly_CheckedChanged);
            _textBoxMobileHomePageUrl.TextChanged += new EventHandler(_textBoxMobileHomePageUrl_TextChanged);
            _textBoxMobilePagesRegex.TextChanged += new EventHandler(_textBoxMobilePagesRegex_TextChanged);
            _checkBoxOriginalUrlAsQueryString.CheckedChanged += new EventHandler(_checkBoxOriginalUrlAsQueryString_CheckedChanged);
            _textBoxTimeout.TextChanged += new EventHandler(_textBoxTimeout_TextChanged);
            _buttonAdd.Click += new EventHandler(_buttonAdd_Click);
            _buttonReset.Click += new EventHandler(_buttonReset_Click);
            _buttonUpdate.Click += new EventHandler(_buttonUpdate_Click);
            _customValidatorRegex.ServerValidate += new ServerValidateEventHandler(_customValidatorRegex_ServerValidate);
        }

        /// <summary>
        /// Sets the final UI elements of the control.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            AddLabel(_panelDevicesFile, 
                this.RedirectLabelDevicesFileText,
                this.RedirectDevicesFileToolTip, 
                null, null);
            AddLabel(_panelEnabled,
                this.RedirectLabelEnabledText,
                this.RedirectEnabledToolTip,
                null, null);
            AddLabel(_panelFirstRequestOnly,
                this.RedirectLabelFirstRequestOnlyText,
                this.RedirectFirstRequestOnlyToolTip,
                null, null);
            AddLabel(_panelMobileHomePageUrl,
                this.RedirectLabelMobileHomePageUrlText,
                this.RedirectMobileRedirectUrlToolTip,
                null, null);
            AddLabel(_panelMobilePagesRegex,
                this.RedirectLabelMobilePagesRegexText,
                this.RedirectMobilePagesRegexToolTip,
                null, null);
            AddLabel(_panelOriginalUrlAsQueryString,
                this.RedirectLabelOriginalUrlAsQueryStringText,
                this.RedirectOriginalUrlAsQueryStringToolTip,
                null, null);
            AddLabel(_panelTimeout,
                this.RedirectLabelTimeoutText,
                this.RedirectTimeoutToolTip,
                null, null);

            AddCssClass(_panelBasic, CssClassBasic);
            AddCssClass(_panelLocations, CssClassLocations);
            if (String.IsNullOrEmpty(_panelMessages.CssClass))
                AddCssClass(_panelMessages, ErrorCssClass);
            AddCssClass(_panelDevicesFile, LabelCssClass);
            AddCssClass(_panelEnabled, LabelCssClass);
            AddCssClass(_panelFirstRequestOnly, LabelCssClass);
            AddCssClass(_panelMobileHomePageUrl, LabelCssClass);
            AddCssClass(_panelMobilePagesRegex, LabelCssClass);
            AddCssClass(_panelOriginalUrlAsQueryString, LabelCssClass);
            AddCssClass(_panelTimeout, LabelCssClass);
            AddCssClass(_textBoxDevicesFile, TextBoxCssClass);
            AddCssClass(_textBoxMobileHomePageUrl, TextBoxCssClass);
            AddCssClass(_textBoxMobilePagesRegex, TextBoxCssClass);
            AddCssClass(_textBoxTimeout, TextBoxCssClass);
            AddCssClass(_checkBoxEnabled, CheckBoxCssClass);
            AddCssClass(_checkBoxFirstRequestOnly, CheckBoxCssClass);
            AddCssClass(_checkBoxOriginalUrlAsQueryString, CheckBoxCssClass);
            AddCssClass(_buttonAdd, ButtonCssClass);
            AddCssClass(_buttonUpdate, ButtonCssClass);
            AddCssClass(_buttonReset, ButtonCssClass);
            AddCssClass(_panelButtons, CssClassButtons);

            _literalBasic.Text = this.RedirectBasicHeadingHtml;
            _literalLocations.Text = this.RedirectLocationsHeadingHtml;

            _buttonReset.Text = this.RedirectButtonResetText;
            _buttonUpdate.Text = this.RedirectButtonUpdateText;
            _buttonAdd.Text = this.RedirectButtonNewLocationText;

            _customValidatorRegex.ErrorMessage = this.RedirectErrorMessageMobileHomePageRegexText;
            _regularExpressionValidatorDevicesFile.ErrorMessage = this.RedirectErrorMessageDevicesFileText;
            _regularExpressionValidatorTimeout.ErrorMessage = this.RedirectErrorMessageTimeOutText;
            _regularExpressionValidatorUrl.ErrorMessage = this.RedirectErrorMessageMobileHomePageUrlText;

            SetError(_regularExpressionValidatorDevicesFile, _textBoxDevicesFile, ErrorCssClass);
            SetError(_regularExpressionValidatorTimeout, _textBoxTimeout, ErrorCssClass);
            SetError(_regularExpressionValidatorUrl, _textBoxMobileHomePageUrl, ErrorCssClass);
            SetError(_customValidatorRegex, _textBoxMobilePagesRegex, ErrorCssClass);

            bool invalid = false;
            foreach (IValidator validator in Page.GetValidators(VALIDATION_GROUP))
            {
                if (validator.IsValid == false)
                {
                    invalid = true;
                    break;
                }
            }
            _panelMessages.Visible =
                String.IsNullOrEmpty(_literalMessage.Text) == false || invalid;
        }

        private void _customValidatorRegex_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = CheckIsRegExValid(args.Value);
        }

        private void _buttonUpdate_Click(object sender, EventArgs e)
        {
            string xml = null;
            try
            {
                if (Page.IsValid)
                {
                    var section = _data.GetElement();
                    xml = section.GetXmlElement();
                    if (section != null)
                    {
                        FiftyOne.Foundation.Mobile.Configuration.Support.SetWebApplicationSection(section);
                        FiftyOne.Foundation.Mobile.Configuration.Manager.Refresh();
                    }
                    _literalMessage.Text = this.RedirectUpdateSuccessHtml;
                    _panelMessages.CssClass = SuccessCssClass;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                _literalMessage.Text = String.Format(
                    this.RedirectUpdateGeneralDetailedFailureHtml,
                    ex.Message,
                    ex.StackTrace,
                    xml);
#else
                _literalMessage.Text = String.Format(
                    this.RedirectUpdateGeneralFailureHtml, 
                    ex.Message,
                    xml == null ? "<i>Not Available</i>" : HttpUtility.HtmlEncode(xml));
#endif
                EventLog.Fatal(ex);
            }
        }

        private void _buttonReset_Click(object sender, EventArgs e)
        {
            _data = new RedirectData(FiftyOne.Foundation.Mobile.Configuration.Manager.Redirect);
            LoadData();
        }

        private void _buttonAdd_Click(object sender, EventArgs e)
        {
            var newLocation = new LocationData(_data);
            _data.Add(newLocation);
            _locationsControl.Rows.Add(new LocationControl(newLocation, this));
        }

        private void _textBoxTimeout_TextChanged(object sender, EventArgs e)
        {
            _data.Timeout = int.Parse(((TextBox)sender).Text);
        }

        private void _checkBoxOriginalUrlAsQueryString_CheckedChanged(object sender, EventArgs e)
        {
            _data.OriginalUrlAsQueryString = ((CheckBox)sender).Checked; ;
        }

        private void _textBoxMobilePagesRegex_TextChanged(object sender, EventArgs e)
        {
            _data.MobilePagesRegex = ((TextBox)sender).Text;
        }

        private void _textBoxMobileHomePageUrl_TextChanged(object sender, EventArgs e)
        {
            _data.MobileHomePageUrl = ((TextBox)sender).Text;
        }

        private void _checkBoxFirstRequestOnly_CheckedChanged(object sender, EventArgs e)
        {
            _data.FirstRequestOnly = ((CheckBox)sender).Checked;
        }

        private void _checkBoxEnabled_CheckedChanged(object sender, EventArgs e)
        {
            _data.Enabled = ((CheckBox)sender).Checked;
        }

        private void _textBoxDevicesFile_TextChanged(object sender, EventArgs e)
        {
            _data.DevicesFile = ((TextBox)sender).Text;
        }

        #endregion
    }
}
