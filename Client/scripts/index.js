async function fetchFiles() {
    try {
        const response = await fetch('http://localhost:5245/blob/files');
        const files = await response.json();

        const fileListElement = document.getElementById('file-list');

        files.forEach(file => {
            const listItem = document.createElement('li');
            listItem.className = 'file-item';

            const fileNameSpan = document.createElement('span');
            fileNameSpan.className = 'file-name';
            fileNameSpan.textContent = file.fileName;

            const buttonGroup = document.createElement('div');
            buttonGroup.className = 'button-group';

            const renameButton = document.createElement('button');
            renameButton.className = 'rename-button';
            renameButton.textContent = 'Rename';
            renameButton.onclick = () => renameFile(file.fileName);

            const downloadButton = document.createElement('button');
            downloadButton.className = 'download-button';
            downloadButton.textContent = 'Download';
            downloadButton.onclick = () => downloadFile(file.fileName);

            listItem.appendChild(fileNameSpan);
            listItem.appendChild(buttonGroup);
            buttonGroup.appendChild(renameButton);
            buttonGroup.appendChild(downloadButton);

            fileListElement.appendChild(listItem);
        });
    } catch (error) {
        console.error('Error fetching files:', error);
    }
}

async function renameFile(oldName) {
    let newName = prompt(`Rename "${oldName}" to:`);
    if (newName) {
        const jwtToken = localStorage.getItem('jwtToken');

        $.ajax({
            url: `http://localhost:5245/blob/${oldName}/rename?newName=${newName}`,
            type: 'PUT',
            headers: {
                'Authorization': `Bearer ${jwtToken}`
            },
            success: function(response) {
                alert(`"${oldName}" has been renamed to "${newName}".`);
                document.getElementById('file-list').innerHTML = '';
                fetchFiles();
            },
            error: function(jqXHR, textStatus, errorThrown) {
                alert(`Failed to rename "${oldName}".`);
                console.error('Error renaming file:', errorThrown);
            }
        });
    }
}

function downloadFile(fileName) {
    const downloadUrl = `http://localhost:5245/blob/download?fileName=${encodeURIComponent(fileName)}`;
    const link = document.createElement('a');
    link.href = downloadUrl;
    link.download = fileName;
    link.click();
}

async function uploadFile(file) {
    let formData = new FormData();
    formData.append('file', file);
    let jwtToken = localStorage.getItem('jwtToken');

    await $.ajax({
        url: 'http://localhost:5245/blob/upload',
        type: 'POST',
        data: formData,
        headers: {
            'Authorization': `Bearer ${jwtToken}`
        },
        contentType: false,
        processData: false,
        success: function(response) {
            console.log('File uploaded successfully');
            document.getElementById('file-list').innerHTML = '';
            fetchFiles();
        },
        error: function(jqXHR, textStatus, errorThrown) {
            console.error('Error uploading file:', errorThrown);
            alert(`Failed to upload "${file.name}".`);
        }
    });
}