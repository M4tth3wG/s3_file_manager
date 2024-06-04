import React, { useState, useEffect, useRef } from "react";
import './css/FileList.css';
import { API_URL } from "../App";
import Modal from "./Modal";

function FileList({ files, setFiles, fetchFiles }){
    const [showModal, setShowModal] = useState(false);
    const [userInput, setUserInput] = useState('');
    const [currentFile, setCurrentFile] = useState(null);

    const handleOpenModal = (file) => {
      setCurrentFile(file);
      setUserInput(file.name);
      setShowModal(true);
    };

    const handleCloseModal = () => setShowModal(false);

    const handleSaveInput = (input) => {
      if (currentFile) {
        const renameFile = async () => {
          try {
              const response = await fetch(`${API_URL}/file/edit?fileId=${currentFile.id}&newName=${input}`,
              {
                  method: 'GET',
              });
            if (!response.ok) {
              throw new Error('Failed to rename file.');
            }

            setFiles(files.map(file => 
              file.id === currentFile.id ? { ...file, name: input } : file
            ));

          } catch (error) {
            console.log(error.message);
          };
        }
        renameFile(); 
      }
      setShowModal(false);
    };


    const handleDownload = (file) => {
      const downloadFile = async () => {
        try {
            const response = await fetch(file.fileLink, {
                method: 'GET',
            });

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            const blob = await response.blob();
            const downloadUrl = window.URL.createObjectURL(blob);
            const anchor = document.createElement('a');
            anchor.href = downloadUrl;
            anchor.setAttribute('download', file.name); // Desired file name
            document.body.appendChild(anchor);
            anchor.click();
            document.body.removeChild(anchor);
            window.URL.revokeObjectURL(downloadUrl);
        } catch (error) {
            console.error('Download failed:', error);
        }
      };
      downloadFile();
    };
    
    const handleEdit = (file) => {
      handleOpenModal(file);
    };
    
    const handleDelete = (fileId) => {
        const deleteFile = async () => {
            try {
                const response = await fetch(`${API_URL}/file/delete?fileId=${fileId}`,
                {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json'
                    }
                });

                if (!response.ok) {
                    throw new Error('Failed to fetch data');
                }

                setFiles(files.filter(file => file.id !== fileId));
            } catch (error) {
                console.log(error.message);
            };
        };
        deleteFile();
    }

    return (
    <div className="file-table">
      <h2>Przesłane pliki</h2>
      {files && files.length > 0 ?
        (
          <table>
          <thead>
            <tr>
              <th>Id</th>
              <th>Nazwa pliku</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {files.map(file => (
              <tr key={file.id}>
                <td>{file.id}</td>
                <td>{file.name}</td>
                <td>
                  <span className="material-symbols-outlined icon" onClick={() => handleDownload(file)}>download</span>
                  <span className="material-symbols-outlined icon" onClick={() => handleEdit(file)}>edit</span>
                  <span className="material-symbols-outlined icon" onClick={() => handleDelete(file.id)}>delete</span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
         ) :
         (
          <p>Brak plików</p>
         )
        } 
      <Modal show={showModal} handleClose={handleCloseModal} handleSave={handleSaveInput} title={"Edytuj nazwę pliku"} defaultValue={userInput}/>
    </div>
  );
}

export default FileList;