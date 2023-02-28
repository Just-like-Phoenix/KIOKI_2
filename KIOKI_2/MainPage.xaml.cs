using KIOKI_2.Classes;

namespace KIOKI_2;

public partial class MainPage : ContentPage
{
    FileResult fileResult;

    public MainPage()
	{
		InitializeComponent();
	}

    private async void Button_FileChoose_Clicked(object sender, EventArgs e)
    {
        var customFileType = new FilePickerFileType(
            new Dictionary<DevicePlatform, IEnumerable<string>> 
            {
                {DevicePlatform.WinUI, new [] { ".txt" } }
            });

        PickOptions options = new()
        {
            PickerTitle = "Выберите текстовы файл",
            FileTypes = customFileType
        };

        fileResult = await FilePicker.PickAsync(options);
        
        if (fileResult != null) 
        {
            if (fileResult.FileName.EndsWith("txt", StringComparison.OrdinalIgnoreCase))
            {
                text.Text = File.ReadAllText(fileResult.FullPath);
            }
        }
    }

    private async void Button_Action_Clicked(object sender, EventArgs e)
    {
        SDES sDES = new SDES(key.Text);
        if (encrypt.IsChecked)
        {
            text.Text = await sDES.EncryptAsync(text.Text);
            if (fileResult != null)
            {
                File.WriteAllText(fileResult.FullPath, text.Text);
                fileResult = null;
            }
        }
        if (decrypt.IsChecked)
        {
            text.Text = await sDES.DecryptAsync(text.Text);
            if (fileResult != null)
            {
                File.WriteAllText(fileResult.FullPath, text.Text);
                fileResult = null;
            }
        }
    }
}