using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SaveLinesInOtherDocument.Infrastructure;

namespace SaveLinesInOtherDocument.ViewModels
{
    internal class MainWindowViewModel : Base.ViewModel
    {
        private RevitModelForfard _revitModel;

        internal RevitModelForfard RevitModel
        {
            get => _revitModel;
            set => _revitModel = value;
        }

        #region Заголовок
        private string _title = "Копировать линии";

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        #endregion

        #region Выбранные линии
        private string _linesElemIds;
        public string LinesElemIds
        {
            get => _linesElemIds;
            set => Set(ref _linesElemIds, value);
        }
        #endregion

        #region Команды

        #region Получение линий
        public ICommand GetLinesCommand { get; }

        private void OnGetLinesCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetLinesBySelection();
            LinesElemIds = RevitModel.LinesElemIds;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetLinesCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Сохранение линий в файл
        public ICommand SaveLinesCommand { get; }

        private void OnSaveLinesCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.SaveLines();
            SaveSettings();
            RevitCommand.mainView.Close();
        }

        private bool CanSaveLinesCommandExecute(object parameter)
        {
            if (string.IsNullOrEmpty(LinesElemIds))
            {
                return false;
            }

            return true;
        }
        #endregion

        #region Закрыть окно
        public ICommand CloseWindowCommand { get; }

        private void OnCloseWindowCommandExecuted(object parameter)
        {
            SaveSettings();
            RevitCommand.mainView.Close();
        }

        private bool CanCloseWindowCommand(object parameter)
        {
            return true;
        }
        #endregion

        #endregion

        private void SaveSettings()
        {
            Properties.Settings.Default.LinesElemIds = LinesElemIds;
            Properties.Settings.Default.Save();
        }


        #region Конструктор класса MainWindowViewModel
        public MainWindowViewModel(RevitModelForfard revitModel)
        {
            RevitModel = revitModel;

            #region Инициализация элементов из Settings

            #region Инициализация линий
            if (!(Properties.Settings.Default.LinesElemIds is null))
            {
                string linesElemIdsInSettings = Properties.Settings.Default.LinesElemIds;
                if (RevitModel.IsLinesExistInModel(linesElemIdsInSettings) && !string.IsNullOrEmpty(linesElemIdsInSettings))
                {
                    LinesElemIds = linesElemIdsInSettings;
                    RevitModel.GetLinesBySettings(linesElemIdsInSettings);
                }
            }

            #endregion

            #endregion

            #region Команды

            GetLinesCommand = new LambdaCommand(OnGetLinesCommandExecuted, CanGetLinesCommandExecute);

            SaveLinesCommand = new LambdaCommand(OnSaveLinesCommandExecuted, CanSaveLinesCommandExecute);

            CloseWindowCommand = new LambdaCommand(OnCloseWindowCommandExecuted, CanCloseWindowCommand);

            #endregion
        }
        #endregion

        public MainWindowViewModel() { }
    }
}
