import React, { useState, useEffect, useRef } from 'react';
import { API_URL } from '../App';
import './css/FileUpload.css'

const FileUpload = ({ fetchFiles }) => {
    const [fileName, setFileName] = useState('');
    const [file, setFile] = useState(null);
    const [verificationToken, setVerificationToken] = useState('');
    const fileInputRef = useRef(null);

    useEffect(() => {
        setFileName(file ? file.name : "")
    }, [file]);

    useEffect(() => {
        const fetchVerificationToken = async () => {
            try {
                const response = await fetch(`${API_URL}/antiforgery/token`,{
                    credentials: 'include',
                    method: 'GET',
                });
                if (response.ok) {
                    const token = await response.text();
                    setVerificationToken(token);
                } else {
                    console.error('Failed to fetch verification token');
                }
            } catch (error) {
                console.error('Error fetching verification token:', error);
            }
        };

        fetchVerificationToken();
    }, []);

    const handleFileNameChange = (event) => {
        setFileName(event.target.value);
    };

    const handleFileChange = (event) => {
        setFile(event.target.files[0]);
    };

    const handleSubmit = async () => {
        if (!file) {
            console.error('No file selected');
           return; 
        }

        const formData = new FormData();
        formData.append('file', file);

        try {
            const response = await fetch(`${API_URL}/file/upload?fileName=${fileName}`, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': verificationToken,
                },
                body: formData,
                credentials: 'include'
            });

            if (response.ok) {
                console.log('File uploaded successfully');
                fetchFiles();
            } else {
                console.error('Failed to upload file');
            }
        } catch (error) {
            console.error('Error uploading file:', error);
        }

        setFile(null);
        fileInputRef.current.value = "";
    };

    return (
        <div className="file-upload-container">
            <div className="input-wrapper">
                <label htmlFor="file-name-input">Nazwa pliku</label>
                <input
                    type="text"
                    id="file-name-input"
                    placeholder="Wprowadź nazwę pliku"
                    value={fileName}
                    onChange={handleFileNameChange}
                    className="file-name-input"
                />
            </div>
            <input
                type="file"
                onChange={handleFileChange}
                ref={fileInputRef}
                className="file-input"
            />
            <button onClick={handleSubmit} className="submit-button">Wyślij</button>
        </div>
    );
};

export default FileUpload;
