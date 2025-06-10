window.renderQrCode = (elementId, qrCodeUri) => {
       const container = document.getElementById(elementId);
       if (!container) return;
       container.innerHTML = ""; // Clear previous QR code if any
       new QRCode(container, {
           text: qrCodeUri,
           width: 200,
           height: 200
       });
};
window.downloadFileFromBytes = (fileName, contentType, bytes) => {
    // Convert .NET byte[] (Uint8Array) to a Blob and trigger download
    const blob = new Blob([new Uint8Array(bytes)], { type: contentType });
    const url = URL.createObjectURL(blob);

    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = fileName;
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);

    setTimeout(() => URL.revokeObjectURL(url), 1000);
};