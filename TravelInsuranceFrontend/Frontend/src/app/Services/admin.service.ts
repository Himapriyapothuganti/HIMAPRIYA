import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class AdminService {
    private apiUrl = 'https://localhost:7161/api/Admin';
    constructor(private http: HttpClient) { }

    getDashboardStats(): Observable<any> {
        return this.http.get(`${this.apiUrl}/dashboard`);
    }

    getUsers(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/users`);
    }

    createUser(userData: any): Observable<any> {
        return this.http.post(`${this.apiUrl}/create-user`, userData);
    }

    toggleUserStatus(userId: string, isActive: boolean): Observable<any> {
        return this.http.put(`${this.apiUrl}/users/${userId}/status?isActive=${isActive}`, {});
    }

    getPolicyProducts(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/policy-products`);
    }

    createPolicyProduct(productData: any): Observable<any> {
        return this.http.post(`${this.apiUrl}/policy-products`, productData);
    }

    togglePolicyProductStatus(id: number, isActive: boolean): Observable<any> {
        return this.http.put(`${this.apiUrl}/policy-products/${id}/status?isActive=${isActive}`, {});
    }

    deletePolicyProduct(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/policy-products/${id}`);
    }

    assignAgentToPolicy(assignmentData: any): Observable<any> {
        return this.http.put(`${this.apiUrl}/assign-agent`, assignmentData);
    }
}
