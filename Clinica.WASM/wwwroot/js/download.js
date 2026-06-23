window.saveAsFile = function (filename, bytesBase64, contentType = 'application/pdf') {
    // 1. Decodificar la cadena Base64
    const byteCharacters = atob(bytesBase64);

    // 2. Asignación directa: Asigna los bytes directamente al Uint8Array. 
    // Esto reduce el consumo de memoria a la mitad y acelera el procesamiento.
    const byteArray = new Uint8Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteArray[i] = byteCharacters.codePointAt(i) ?? 0;
    }

    // 3. Crear el objeto binario (Blob) con tipo de contenido parametrizado
    const blob = new Blob([byteArray], { type: contentType });

    // 4. Crear y disparar el enlace de descarga oculto
    const link = document.createElement('a');
    const blobUrl = URL.createObjectURL(blob);
    link.href = blobUrl;
    link.download = filename;

    document.body.appendChild(link);
    link.click();

    // 5. Limpieza del DOM
    document.body.removeChild(link);

    // 6. Revocar el objeto con un retraso de 100ms.
    // Esto garantiza que los hilos de descarga de celulares no interrumpan la lectura del archivo.
    setTimeout(() => URL.revokeObjectURL(blobUrl), 100);
};