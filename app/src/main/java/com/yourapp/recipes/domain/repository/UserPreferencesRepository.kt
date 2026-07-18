package com.yourapp.recipes.domain.repository

import com.yourapp.recipes.domain.model.UserPreferences
import kotlinx.coroutines.flow.Flow

interface UserPreferencesRepository {
    fun getPreferences(): Flow<UserPreferences>
    suspend fun updateThemeMode(themeMode: String)
    suspend fun updateMeasurementSystem(system: String)
    suspend fun savePreferences(preferences: UserPreferences)
}
