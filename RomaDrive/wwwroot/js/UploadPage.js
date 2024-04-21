var dropZone = document.getElementById('dropZone');
var progressBarContainer = document.getElementById('progressBarContainer');
var isUploadStart = false;

dropZone.addEventListener('dragover', function (e) {
    e.preventDefault();
    dropZone.style.backgroundColor = '#f2f2f2';
});

dropZone.addEventListener('dragleave', function (e) {
    e.preventDefault();
    dropZone.style.backgroundColor = '#ffffff';
});

dropZone.addEventListener('drop', function (e) {
    e.preventDefault();
    dropZone.style.backgroundColor = '#ffffff';

    if (!isUploadStart) {
        isUploadStart = true;
        document.getElementById('progressBar').style.width = '0%';
        document.getElementById('progressBar').innerHTML = '0%';
        document.getElementById('progressBar').style.backgroundColor = '#4CAF50';
        var files = e.dataTransfer.files;
        if (files.length > 0) {
            isExistsFile(files, 0);
        }
    }
});

function isExistsFile(files, index) {
    if (index >= files.length) {
        // Всі файли перевірені
        handleFiles(files, 0, false);
        return;
    }

    var file = files[index];
    var xhr1 = new XMLHttpRequest();
    var formData = new FormData();
    formData.append("fileName", file.name);

    xhr1.open('POST', '/Upload/IsExists', true);
    xhr1.onload = function () {
        if (xhr1.status !== 200) {
            var result = confirm("Файл з такою назвою вже існує. Чи хочете ви перезаписати цей файл?");
            handleFiles(files, index, true, result);
        }
        else { 
            handleFiles(files, index, false);
        }
    };
    xhr1.onerror = function () {
        alert("Error occurred while checking file existence: " + xhr1.response);
    };
    xhr1.send(formData);
}

function handleFiles(files, index, isRewriteFile, isContinue = true) {
    var chunkSize = 1024 * 1024; // 1MB chunks
    progressBarContainer.style.display = 'flex';
    var file = files[index];
    var totalChunks = Math.ceil(file.size / chunkSize);
    var currentChunk = 0;

    if (!isContinue) {
        index++;
        if (index < files.length) {
            isExistsFile(files, index);
        } else {
            isUploadStart = false;
            document.getElementById('progressBar').style.backgroundColor = 'green';
        }
        return;
    }

    var uploadNextChunk = function (start, end) {
        if (currentChunk >= totalChunks) {
            // Всі частини файлу завантажені
            index++;
            if (index < files.length) {
                isExistsFile(files, index);
            } else {
                isUploadStart = false;
                document.getElementById('progressBar').style.backgroundColor = 'green';
            }
            return;
        }

        var xhr = new XMLHttpRequest();
        var formData = new FormData();

        formData.append("file", file.slice(start, end));
        formData.append("fileName", file.name);
        formData.append("chunkNumber", currentChunk);
        formData.append("totalChunks", totalChunks);
        formData.append("isRewriteFile", isRewriteFile);

        xhr.open('POST', '/Upload/Chunk', true);

        xhr.onload = function () {
            if (xhr.status === 200) {
                currentChunk++;
                var progress = Math.round((currentChunk / totalChunks) * 100);
                document.getElementById('progressBar').style.width = progress + '%';
                document.getElementById('progressBar').innerHTML = progress + '%';

                var nextStart = end;
                var nextEnd = Math.min(end + chunkSize, file.size);
                uploadNextChunk(nextStart, nextEnd);
            } else {
                alert("Error occurred while uploading: " + xhr.response);
            }
        };

        xhr.onerror = function () {
            alert("Error occurred while uploading: " + xhr.response);
        };

        xhr.send(formData);
    };

    // Початок відправки першої частини файлу
    uploadNextChunk(0, chunkSize);
}
