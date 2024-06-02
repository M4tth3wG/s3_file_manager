import logo from './logo.svg';
import './App.css';
import FileList from './components/FileList';
import FileUpload from './components/FIleUpload';
import React from 'react'
import { useState, useEffect } from 'react';

export const API_URL = process.env.REACT_APP_API_URL;

function App() {
  const [files, setFiles] = useState([]);

  const fetchFiles = async () => {
    try {
      const response = await fetch(`${API_URL}/file/list`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      if (!response.ok) {
        throw new Error('Failed to fetch data');
      }
      const jsonData = await response.json();
      setFiles(jsonData);
    } catch (error) {
      console.log(error.message);
    }
  };

  useEffect(() => {
    fetchFiles();
  }, []);

  return (
    <div className="App">
      <div className="file-list-container">
        <FileList files={files} setFiles={setFiles} fetchFiles={fetchFiles} />
      </div>
      <div className="file-upload">
        <FileUpload fetchFiles={fetchFiles} />
      </div>
    </div>
  );
}

export default App;
