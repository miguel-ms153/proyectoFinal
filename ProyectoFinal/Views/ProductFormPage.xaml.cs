using ProyectoFinal.Models; using ProyectoFinal.Services;
namespace ProyectoFinal.Views;
public partial class ProductFormPage : ContentPage
{
 private readonly ProductStore store = new();
 public ProductFormPage() { InitializeComponent(); ExpirationPicker.Date = DateTime.Today.AddDays(7); QuantityStepper.ValueChanged += (_, e) => QuantityLabel.Text = $"Cantidad: {(int)e.NewValue}"; }
 private async void CameraClicked(object sender, EventArgs e) { try { var photo = await MediaPicker.CapturePhotoAsync(); CameraStatus.Text = photo is null ? "No se tomó fotografía." : "Foto capturada correctamente."; } catch { CameraStatus.Text = "La cámara no está disponible en este dispositivo."; } }
 private async void SaveClicked(object sender, EventArgs e) { if (string.IsNullOrWhiteSpace(NameEntry.Text)) { await DisplayAlert("Dato requerido", "Ingresa el nombre del producto.", "Aceptar"); return; } await store.SaveAsync(new ProductItem { Name = NameEntry.Text.Trim(), Category = CategoryPicker.SelectedItem?.ToString() ?? "Alimentos", Quantity = (int)QuantityStepper.Value, ExpirationDate = ExpirationPicker.Date ?? DateTime.Today }); await DisplayAlert("Guardado", "El producto se agregó al inventario.", "Aceptar"); NameEntry.Text = string.Empty; }
}
