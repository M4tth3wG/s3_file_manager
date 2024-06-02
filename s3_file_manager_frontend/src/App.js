import logo from './logo.svg';
import './App.css';
import FileList from './components/FileList';
import FileUpload from './components/FIleUpload';
import React from 'react'

export const API_URL = process.env.REACT_APP_API_URL;

function App() {
  return (
    <div className="App">
      <FileList/>
      <FileUpload/>
    </div>
  );
}

export default App;
