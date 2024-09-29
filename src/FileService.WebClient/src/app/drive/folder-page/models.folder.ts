export interface FolderShortModel {
    id: number;
    name: string;
}
  
export interface FileShortModel {
    id: number;
    name: string;
    size: string;
    creationDate: string;
}
  
export interface FolderModel {
    id: number;
    name: string;
    innerFolders: FolderShortModel[];
    files: FileShortModel[];
}

export interface UserShortModel {
    id: string,
    userName: string,
    email: string
}
export interface AccessModel {
    folderName: string,
    permissions: string,
    user: UserShortModel
}
  