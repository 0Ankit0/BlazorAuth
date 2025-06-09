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