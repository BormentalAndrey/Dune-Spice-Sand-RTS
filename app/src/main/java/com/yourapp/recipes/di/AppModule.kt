package com.yourapp.recipes.di

import com.google.gson.Gson
import com.google.gson.GsonBuilder
import com.yourapp.recipes.data.repository.MealPlanRepositoryImpl
import com.yourapp.recipes.data.repository.RecipeRepositoryImpl
import com.yourapp.recipes.data.repository.ShoppingListRepositoryImpl
import com.yourapp.recipes.data.repository.UserPreferencesRepositoryImpl
import com.yourapp.recipes.domain.repository.MealPlanRepository
import com.yourapp.recipes.domain.repository.RecipeRepository
import com.yourapp.recipes.domain.repository.ShoppingListRepository
import com.yourapp.recipes.domain.repository.UserPreferencesRepository
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
object AppModule {
    
    @Provides
    @Singleton
    fun provideGson(): Gson {
        return GsonBuilder()
            .setPrettyPrinting()
            .create()
    }
    
    @Provides
    @Singleton
    fun provideRecipeRepository(
        repository: RecipeRepositoryImpl
    ): RecipeRepository = repository
    
    @Provides
    @Singleton
    fun provideShoppingListRepository(
        repository: ShoppingListRepositoryImpl
    ): ShoppingListRepository = repository
    
    @Provides
    @Singleton
    fun provideMealPlanRepository(
        repository: MealPlanRepositoryImpl
    ): MealPlanRepository = repository
    
    @Provides
    @Singleton
    fun provideUserPreferencesRepository(
        repository: UserPreferencesRepositoryImpl
    ): UserPreferencesRepository = repository
}
