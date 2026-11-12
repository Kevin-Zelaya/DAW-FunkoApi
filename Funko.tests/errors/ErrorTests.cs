
using NUnit.Framework;

[TestFixture]
public class ErrorTests
{
    #region StorageError

    [Test]
    public void ArchivoVacio_ReturnsBusinessRuleError()
    {
        var error = StorageError.ArchivoVacio();

        Assert.That(error, Is.TypeOf<BusinessRuleError>());
        Assert.That(error.Message, Is.EqualTo(
            "El archivo esta vacio o no se ha proporcionado"));
    }

    [Test]
    public void ArchivoMuyGrande_ReturnsBusinessRuleError()
    {
        var error = StorageError.ArchivoMuyGrande();

        Assert.That(error, Is.TypeOf<BusinessRuleError>());
        Assert.That(error.Message, Is.EqualTo(
            "El archivo supera el tamano maximo permitido (5 MB)"));
    }

    [Test]
    public void ExtensionNoPermitida_ReturnsBusinessRuleError()
    {
        var error = StorageError.ExtensionNoPermitida();

        Assert.That(error, Is.TypeOf<BusinessRuleError>());
        Assert.That(error.Message, Is.EqualTo(
            "La extension del archivo no esta permitida. Use: .jpg, .jpeg, .png, .gif"));
    }

    [Test]
    public void TipoContenidoNoPermitido_ReturnsBusinessRuleError()
    {
        var error = StorageError.TipoContenidoNoPermitido();

        Assert.That(error, Is.TypeOf<BusinessRuleError>());
        Assert.That(error.Message, Is.EqualTo(
            "El tipo de contenido no esta permitido. Solo se permiten imagenes"));
    }

    [Test]
    public void NombreArchivoInvalido_ReturnsBusinessRuleError()
    {
        var error = StorageError.NombreArchivoInvalido();

        Assert.That(error, Is.TypeOf<BusinessRuleError>());
        Assert.That(error.Message, Is.EqualTo(
            "El nombre del archivo contiene caracteres no validos"));
    }

    [Test]
    public void ErrorGuardando_ReturnsInternalError()
    {
        var error = StorageError.ErrorGuardando();

        Assert.That(error, Is.TypeOf<InternalError>());
        Assert.That(error.Message, Is.EqualTo(
            "Error al guardar el archivo en el servidor"));
    }

    [Test]
    public void ErrorEliminando_ReturnsInternalError()
    {
        var error = StorageError.ErrorEliminando();

        Assert.That(error, Is.TypeOf<InternalError>());
        Assert.That(error.Message, Is.EqualTo(
            "Error al eliminar el archivo del servidor"));
    }

    #endregion

    #region UserError

    [Test]
    public void UserNotFound_WithId_ReturnsNotFoundError()
    {
        var error = UserError.NotFound(10L);

        Assert.That(error, Is.TypeOf<NotFoundError>());
    }

    [Test]
    public void UserNotFound_WithEmail_ReturnsNotFoundError()
    {
        var error = UserError.NotFound("test@email.com");

        Assert.That(error, Is.TypeOf<NotFoundError>());
    }

    [Test]
    public void UserNombreDuplicado_ReturnsConflictError()
    {
        var error = UserError.NombreDuplicado("Kevin");

        Assert.That(error, Is.TypeOf<ConflictError>());
    }

    #endregion

    #region FunkoError

    [Test]
    public void FunkoNotFound_WithId_ReturnsNotFoundError()
    {
        var error = FunkoError.NotFound(10L);

        Assert.That(error, Is.TypeOf<NotFoundError>());
    }

    [Test]
    public void FunkoNotFound_WithEmail_ReturnsNotFoundError()
    {
        var error = FunkoError.NotFound("funko@email.com");

        Assert.That(error, Is.TypeOf<NotFoundError>());
    }

    [Test]
    public void FunkoNombreDuplicado_ReturnsConflictError()
    {
        var error = FunkoError.NombreDuplicado("Spider-Man");

        Assert.That(error, Is.TypeOf<ConflictError>());
    }

    #endregion
}
