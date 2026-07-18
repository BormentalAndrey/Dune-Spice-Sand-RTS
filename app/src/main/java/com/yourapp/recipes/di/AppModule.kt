package com.yourapp.recipes.di

import com.google.gson.Gson
import com.google.gson.GsonBuilder
import com.yourapp.recipes.data.repository.RecipeRepositoryImpl
import com.yourapp.recipes.data.repository.ShoppingListRepositoryImpl
import com.yourapp.recipes.domain.repository.RecipeRepository
import com.yourapp.recipes.domain.repository.ShoppingListRepository
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
}
