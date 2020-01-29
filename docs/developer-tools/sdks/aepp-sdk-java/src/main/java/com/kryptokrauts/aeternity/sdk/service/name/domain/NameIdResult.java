package com.kryptokrauts.aeternity.sdk.service.name.domain;

import com.kryptokrauts.aeternity.generated.model.NameEntry;
import com.kryptokrauts.aeternity.generated.model.NamePointer;
import com.kryptokrauts.aeternity.sdk.constants.ApiIdentifiers;
import com.kryptokrauts.aeternity.sdk.domain.GenericResultObject;
import java.math.BigInteger;
import java.util.LinkedList;
import java.util.List;
import java.util.Optional;
import java.util.stream.Collectors;
import lombok.Getter;
import lombok.ToString;
import lombok.experimental.SuperBuilder;

@Getter
@SuperBuilder(toBuilder = true)
@ToString
public class NameIdResult extends GenericResultObject<NameEntry, NameIdResult> {

  private String id;
  private BigInteger ttl;
  private List<String> pointers;

  public Optional<String> getAccountPointer() {
    return pointers.stream().filter(p -> p.startsWith(ApiIdentifiers.ACCOUNT_PUBKEY)).findFirst();
  }

  public Optional<String> getChannelPointer() {
    return pointers.stream().filter(p -> p.startsWith(ApiIdentifiers.CHANNEL)).findFirst();
  }

  public Optional<String> getContractPointer() {
    return pointers.stream().filter(p -> p.startsWith(ApiIdentifiers.CONTRACT_PUBKEY)).findFirst();
  }

  public Optional<String> getOraclePointer() {
    return pointers.stream().filter(p -> p.startsWith(ApiIdentifiers.ORACLE_PUBKEY)).findFirst();
  }

  @Override
  protected NameIdResult map(NameEntry generatedResultObject) {
    if (generatedResultObject != null)
      return this.toBuilder()
          .id(generatedResultObject.getId())
          .ttl(generatedResultObject.getTtl())
          .pointers(getPointers(generatedResultObject.getPointers()))
          .build();
    else return this.toBuilder().build();
  }

  @Override
  protected String getResultObjectClassName() {
    return NameIdResult.class.getName();
  }

  private List<String> getPointers(final List<NamePointer> namePointers) {
    if (namePointers == null) {
      return new LinkedList<>();
    }
    return namePointers.stream().map(pointer -> pointer.getId()).collect(Collectors.toList());
  }
}
